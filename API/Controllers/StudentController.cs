using API.Common;
using API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class StudentController : BaseApiController
    {
        private readonly ISqlQueryHelper _sqlQueryHelper;

        public StudentController(ISqlQueryHelper sqlQueryHelper)
        {
            _sqlQueryHelper = sqlQueryHelper;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel>> Get()
        {
            try
            {
                string sql = @"
SELECT
    s.*,
    selectedCompany.CompanyName AS SelectedCompanyName
FROM Students s WITH(NOLOCK)
OUTER APPLY
(
    SELECT TOP 1 c.CompanyName
    FROM JobApplications ja WITH(NOLOCK)
    INNER JOIN Companies c WITH(NOLOCK) ON c.Id = ja.CompanyId
    WHERE ja.StudentId = s.Id
      AND ja.Status = 'accepted'
    ORDER BY ja.DecisionAt DESC, ja.UpdatedAt DESC, ja.ApplicationId DESC
) selectedCompany";
                List<StudentModel>? studentList = await _sqlQueryHelper.GetListAsync<StudentModel>(sql);
                for (int i = 0; i < studentList.Count; i++)
                {
                    var student = studentList[i];
                    string sql2 = @"
SELECT
    UserId,
    UserName,
    Role,
    Status,
    ProfileImagePath,
    Email,
    MobileNo,
    StreetAddress,
    City,
    District,
    State,
    Country,
    PinCode,
    IsDeleted,
    CreatedAt,
    UpdatedAt
FROM Users WITH(NOLOCK)
WHERE UserId = @UserId";
                    UserModel? user = await _sqlQueryHelper.GetSingleAsync<UserModel>(sql2, new { student.UserId });
                    studentList[i].User = user;
                }

                return Success("Students retrieved successfully.", studentList ?? []);
            }
            catch (Exception ex)
            {
                return Failure($"Error - {ex.Message}");
            }
        }

        [HttpGet("{studentId}")]
        public async Task<ActionResult<ResponseModel>> Get(long studentId)
        {
            try
            {
                if (studentId <= 0)
                {
                    return Failure("Student id is required.");
                }

                string sql = "SELECT * FROM Students With(nolock) where Id = @id";
                StudentModel? student = await _sqlQueryHelper.GetSingleAsync<StudentModel>(sql, new { id = studentId });
                if (student == null)
                {
                    return Failure("Student not found.");
                }

                return Success("Student retrieved successfully.", student);
            }
            catch (Exception ex)
            {
                return Failure($"Error - {ex.Message}");
            }
        }

        [HttpGet("dashboard/{studentId:long}")]
        public async Task<ActionResult<ResponseModel>> GetDashboard(long studentId)
        {
            try
            {
                if (studentId <= 0)
                {
                    return Failure("Student id is required.");
                }

                await EnsureJobApplicationsTableAsync();

                StudentModel? student = await _sqlQueryHelper.GetSingleAsync<StudentModel>(@"
SELECT TOP 1 *
FROM Students WITH(NOLOCK)
WHERE Id = @StudentId", new { StudentId = studentId });

                if (student == null)
                {
                    return Failure("Student not found.");
                }

                UserModel? user = await _sqlQueryHelper.GetSingleAsync<UserModel>(@"
SELECT TOP 1 *
FROM Users WITH(NOLOCK)
WHERE UserId = @UserId", new { student.UserId });

                List<StudentDashboardRecentApplicationModel> recentApplications =
                    await _sqlQueryHelper.GetListAsync<StudentDashboardRecentApplicationModel>(@"
SELECT TOP 5
    ja.ApplicationId,
    ja.JobId,
    c.CompanyName AS Company,
    j.JobTitle AS Role,
    ja.Status,
    ja.AppliedAt
FROM JobApplications ja WITH(NOLOCK)
INNER JOIN Jobs j WITH(NOLOCK) ON j.JobId = ja.JobId
INNER JOIN Companies c WITH(NOLOCK) ON c.Id = ja.CompanyId
WHERE ja.StudentId = @StudentId
ORDER BY ja.AppliedAt DESC, ja.ApplicationId DESC", new { StudentId = studentId });

                List<StudentDashboardChartItemModel> applicationStatusChart =
                    await _sqlQueryHelper.GetListAsync<StudentDashboardChartItemModel>(@"
SELECT
    CASE
        WHEN ja.Status = 'interview_scheduled' THEN 'Interview Scheduled'
        WHEN ja.Status = 'accepted' THEN 'Accepted'
        WHEN ja.Status = 'rejected' THEN 'Rejected'
        ELSE 'Applied'
    END AS Label,
    COUNT(1) AS Value
FROM JobApplications ja WITH(NOLOCK)
WHERE ja.StudentId = @StudentId
GROUP BY CASE
    WHEN ja.Status = 'interview_scheduled' THEN 'Interview Scheduled'
    WHEN ja.Status = 'accepted' THEN 'Accepted'
    WHEN ja.Status = 'rejected' THEN 'Rejected'
    ELSE 'Applied'
END
ORDER BY Value DESC, Label ASC", new { StudentId = studentId });

                List<JobModel> activeJobs = await _sqlQueryHelper.GetListAsync<JobModel>(@"
SELECT
    JobId,
    RequiredSkills,
    PreferredSkills,
    Status,
    ExpiryDate
FROM Jobs WITH(NOLOCK)
WHERE Status = 'published'
  AND (ExpiryDate IS NULL OR ExpiryDate >= @CurrentDate)", new { CurrentDate = DateTime.UtcNow });

                int appliedJobsCount = await _sqlQueryHelper.GetSingleAsync<int>(@"
SELECT COUNT(1)
FROM JobApplications WITH(NOLOCK)
WHERE StudentId = @StudentId", new { StudentId = studentId });

                int shortlistedCount = await _sqlQueryHelper.GetSingleAsync<int>(@"
SELECT COUNT(1)
FROM JobApplications WITH(NOLOCK)
WHERE StudentId = @StudentId
  AND Status IN ('interview_scheduled', 'accepted')", new { StudentId = studentId });

                int upcomingDrivesCount = await _sqlQueryHelper.GetSingleAsync<int>(@"
SELECT COUNT(1)
FROM Jobs j WITH(NOLOCK)
WHERE j.Status = 'published'
  AND (j.ExpiryDate IS NULL OR j.ExpiryDate >= @CurrentDate)
  AND NOT EXISTS
  (
      SELECT 1
      FROM JobApplications ja WITH(NOLOCK)
      WHERE ja.JobId = j.JobId
        AND ja.StudentId = @StudentId
  )", new { StudentId = studentId, CurrentDate = DateTime.UtcNow });

                StudentDashboardModel dashboard = new()
                {
                    ProfileCompletionPercentage = CalculateProfileCompletion(student, user),
                    AppliedJobsCount = appliedJobsCount,
                    UpcomingDrivesCount = upcomingDrivesCount,
                    ShortlistedCount = shortlistedCount,
                    RecentApplications = recentApplications,
                    ApplicationStatusChart = applicationStatusChart,
                    SkillDemandChart = BuildSkillDemandChart(activeJobs)
                };

                return Success("Student dashboard retrieved successfully.", dashboard);
            }
            catch (Exception ex)
            {
                return Failure($"Error - {ex.Message}");
            }
        }

        [HttpGet("applications/{studentId:long}")]
        public async Task<ActionResult<ResponseModel>> GetApplications(long studentId)
        {
            try
            {
                if (studentId <= 0)
                {
                    return Failure("Student id is required.");
                }

                await EnsureJobApplicationsTableAsync();

                const string sql = @"
SELECT
    ja.*,
    j.JobTitle,
    j.WorkMode,
    c.CompanyName,
    c.LogoUrl AS CompanyLogoUrl,
    c.Location AS CompanyLocation
FROM JobApplications ja WITH(NOLOCK)
INNER JOIN Jobs j WITH(NOLOCK) ON ja.JobId = j.JobId
INNER JOIN Companies c WITH(NOLOCK) ON ja.CompanyId = c.Id
WHERE ja.StudentId = @StudentId
ORDER BY ja.AppliedAt DESC, ja.ApplicationId DESC";

                List<StudentJobApplicationListItemModel> applications =
                    await _sqlQueryHelper.GetListAsync<StudentJobApplicationListItemModel>(sql, new { StudentId = studentId });

                return Success("Student applications retrieved successfully.", applications);
            }
            catch (Exception ex)
            {
                return Failure($"Error - {ex.Message}");
            }
        }

        private static int CalculateProfileCompletion(StudentModel student, UserModel? user)
        {
            const int totalFields = 14;
            int completedFields = 0;

            if (!string.IsNullOrWhiteSpace(student.FirstName)) completedFields++;
            if (!string.IsNullOrWhiteSpace(student.LastName)) completedFields++;
            if (student.DateOfBirth.HasValue) completedFields++;
            if (!string.IsNullOrWhiteSpace(student.Gender)) completedFields++;
            if (!string.IsNullOrWhiteSpace(student.Nationality)) completedFields++;
            if (!string.IsNullOrWhiteSpace(student.EnrollmentNo)) completedFields++;
            if (!string.IsNullOrWhiteSpace(student.Department)) completedFields++;
            if (student.PassingYear > 0) completedFields++;
            if (student.CGPA > 0) completedFields++;
            if (!string.IsNullOrWhiteSpace(student.ResumeFilePath)) completedFields++;
            if (!string.IsNullOrWhiteSpace(student.Skills)) completedFields++;
            if (!string.IsNullOrWhiteSpace(user?.Email)) completedFields++;
            if (!string.IsNullOrWhiteSpace(user?.MobileNo)) completedFields++;
            if (!string.IsNullOrWhiteSpace(user?.City)) completedFields++;

            return (int)Math.Round((double)completedFields / totalFields * 100, MidpointRounding.AwayFromZero);
        }

        private static List<StudentDashboardChartItemModel> BuildSkillDemandChart(List<JobModel> activeJobs)
        {
            Dictionary<string, int> skillCounts = new(StringComparer.OrdinalIgnoreCase);

            foreach (JobModel job in activeJobs)
            {
                IEnumerable<string> jobSkills = SplitSkills(job.RequiredSkills)
                    .Concat(SplitSkills(job.PreferredSkills));

                foreach (string skill in jobSkills.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (skillCounts.ContainsKey(skill))
                    {
                        skillCounts[skill]++;
                    }
                    else
                    {
                        skillCounts[skill] = 1;
                    }
                }
            }

            return skillCounts
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key)
                .Take(5)
                .Select(x => new StudentDashboardChartItemModel
                {
                    Label = x.Key,
                    Value = x.Value
                })
                .ToList();
        }

        private static IEnumerable<string> SplitSkills(string? skills)
        {
            if (string.IsNullOrWhiteSpace(skills))
            {
                return [];
            }

            return Regex.Split(skills, @"[,/|]+")
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x));
        }

        private async Task EnsureJobApplicationsTableAsync()
        {
            const string sql = @"
IF OBJECT_ID('dbo.JobApplications', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[JobApplications] (
        ApplicationId BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        JobId BIGINT NOT NULL,
        CompanyId BIGINT NOT NULL,
        StudentId BIGINT NOT NULL,
        StudentUserId BIGINT NOT NULL,
        StudentName NVARCHAR(200) NOT NULL,
        StudentEmail NVARCHAR(255) NOT NULL,
        StudentPhone NVARCHAR(30) NULL,
        ResumeFilePath NVARCHAR(500) NULL,
        Status NVARCHAR(30) NOT NULL DEFAULT 'applied',
        AppliedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        InterviewScheduledAt DATETIME2 NULL,
        InterviewMode NVARCHAR(100) NULL,
        InterviewLocation NVARCHAR(300) NULL,
        InterviewNotes NVARCHAR(MAX) NULL,
        DecisionAt DATETIME2 NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_JobApplications_Jobs FOREIGN KEY (JobId) REFERENCES Jobs(JobId) ON DELETE CASCADE,
        CONSTRAINT FK_JobApplications_Companies FOREIGN KEY (CompanyId) REFERENCES Companies(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_JobApplications_Students FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_JobApplications_Users FOREIGN KEY (StudentUserId) REFERENCES Users(UserId) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX IX_JobApplications_JobId_StudentId
    ON JobApplications(JobId, StudentId);
END

IF COL_LENGTH('dbo.JobApplications', 'InterviewScheduledAt') IS NULL
BEGIN
    ALTER TABLE JobApplications ADD InterviewScheduledAt DATETIME2 NULL;
END

IF COL_LENGTH('dbo.JobApplications', 'InterviewMode') IS NULL
BEGIN
    ALTER TABLE JobApplications ADD InterviewMode NVARCHAR(100) NULL;
END

IF COL_LENGTH('dbo.JobApplications', 'InterviewLocation') IS NULL
BEGIN
    ALTER TABLE JobApplications ADD InterviewLocation NVARCHAR(300) NULL;
END

IF COL_LENGTH('dbo.JobApplications', 'InterviewNotes') IS NULL
BEGIN
    ALTER TABLE JobApplications ADD InterviewNotes NVARCHAR(MAX) NULL;
END

IF COL_LENGTH('dbo.JobApplications', 'DecisionAt') IS NULL
BEGIN
    ALTER TABLE JobApplications ADD DecisionAt DATETIME2 NULL;
END";

            const string joiningDateSql = @"
IF COL_LENGTH('dbo.JobApplications', 'JoiningDate') IS NULL
BEGIN
    ALTER TABLE JobApplications ADD JoiningDate DATETIME2 NULL;
END";

            await _sqlQueryHelper.ExecuteAsync(sql);
            await _sqlQueryHelper.ExecuteAsync(joiningDateSql);
        }
    }
}
