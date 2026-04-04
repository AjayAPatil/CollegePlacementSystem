using API.Common;
using API.Constants;
using API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class JobsController : BaseApiController
    {
        private readonly ISqlQueryHelper _sqlQueryHelper;

        public JobsController(ISqlQueryHelper sqlQueryHelper)
        {
            _sqlQueryHelper = sqlQueryHelper;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel>> Get()
        {
            try
            {
                string sql = "SELECT * FROM Jobs WITH (NOLOCK) ORDER BY CreatedAt DESC";
                List<JobModel> jobList = await _sqlQueryHelper.GetListAsync<JobModel>(sql);

                for (int i = 0; i < jobList.Count; i++)
                {
                    JobModel job = jobList[i];
                    job.Company = await _sqlQueryHelper.GetSingleAsync<CompanyModel>(
                        "SELECT * FROM Companies WITH (NOLOCK) WHERE Id = @Id",
                        new { Id = job.CompanyId });
                    job.Creator = await _sqlQueryHelper.GetSingleAsync<UserModel>(
                        "SELECT * FROM Users WITH (NOLOCK) WHERE UserId = @UserId",
                        new { UserId = job.CreatedBy });
                }

                return Success("Jobs retrieved successfully.", jobList);
            }
            catch (Exception ex)
            {
                return Failure($"Error - {ex.Message}");
            }
        }

        [HttpGet("{page}/{pageSize}")]
        public async Task<ActionResult<ResponseModel>> Get(int page = 1, int pageSize = 10, [FromQuery] long? studentId = null)
        {
            try
            {
                await EnsureJobApplicationsTableAsync();

                page = page < 1 ? 1 : page;
                pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 50);
                int offset = (page - 1) * pageSize;

                const string countSql = @"
SELECT COUNT(1)
FROM Jobs j
WHERE j.Status = 'published'";

                string sql = @"
        SELECT 
            j.*,

            c.CompanyName,
            c.LogoUrl,

            u.UserName AS CreatorName,
            CAST(CASE WHEN ja.ApplicationId IS NULL THEN 0 ELSE 1 END AS bit) AS IsApplied

        FROM Jobs j
        LEFT JOIN Companies c ON j.CompanyId = c.Id
        LEFT JOIN Users u ON j.CreatedBy = u.UserId
        LEFT JOIN JobApplications ja ON j.JobId = ja.JobId AND (@StudentId IS NOT NULL AND ja.StudentId = @StudentId)
        WHERE j.Status = 'published'

        ORDER BY j.CreatedAt DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                int totalCount = await _sqlQueryHelper.GetSingleAsync<int>(countSql);
                var jobs = await _sqlQueryHelper.GetListAsync<JobViewModel>(
                    sql,
                    new { Offset = offset, PageSize = pageSize, StudentId = studentId });

                var pagedResult = new PagedResult<JobViewModel>
                {
                    Items = jobs,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    HasMore = offset + jobs.Count < totalCount
                };

                return Ok(new ResponseModel
                {
                    Status = ResponseStatus.Success,
                    Message = "Data Retrived!",
                    Data = pagedResult
                });
            }
            catch(Exception ex)
            {
                return Ok(new ResponseModel
                {
                    Status = ResponseStatus.Failure,
                    Message = ex.Message,
                    Data = new PagedResult<JobViewModel>()
                });
            }
        }

        [HttpGet("details/{jobId:long}")]
        public async Task<ActionResult<ResponseModel>> GetDetails(long jobId)
        {
            try
            {
                string sql = @"
SELECT
    j.*,
    c.CompanyName,
    c.LogoUrl,
    c.Website AS CompanyWebsite,
    c.Description AS CompanyDescription,
    c.Industry AS CompanyIndustry,
    c.Location AS CompanyLocation,
    c.HRName AS CompanyHrName,
    c.ContactEmail AS CompanyContactEmail,
    c.ContactPhone AS CompanyContactPhone,
    c.FoundedYear AS CompanyFoundedYear,
    c.CompanySize,
    u.UserName AS CreatorName
FROM Jobs j
LEFT JOIN Companies c ON j.CompanyId = c.Id
LEFT JOIN Users u ON j.CreatedBy = u.UserId
WHERE j.JobId = @JobId";

                JobDetailViewModel? job = await _sqlQueryHelper.GetSingleAsync<JobDetailViewModel>(
                    sql,
                    new { JobId = jobId });

                if (job == null)
                {
                    return Failure("Job not found.");
                }


                return Success("Job details retrieved successfully.", job);
            }
            catch (Exception ex)
            {
                return Failure(ex.Message, ex);
            }
        }

        [HttpPost("{jobId:long}/apply")]
        public async Task<ActionResult<ResponseModel>> Apply(long jobId, [FromBody] JobApplyRequestModel requestData)
        {
            try
            {
                if (requestData == null || requestData.StudentId <= 0)
                {
                    return Failure("Student is required.");
                }

                await EnsureJobApplicationsTableAsync();

                JobDetailViewModel? job = await _sqlQueryHelper.GetSingleAsync<JobDetailViewModel>(
                    @"SELECT j.JobId, j.CompanyId, j.Status, j.JobTitle
FROM Jobs j
WHERE j.JobId = @JobId",
                    new { JobId = jobId });

                if (job == null)
                {
                    return Failure("Job not found.");
                }

                if (!string.Equals(job.Status, JobStatus.Published, StringComparison.OrdinalIgnoreCase))
                {
                    return Failure("Only published jobs can be applied to.");
                }

                var student = await _sqlQueryHelper.GetSingleAsync<StudentModel>(
                    "SELECT * FROM Students WITH (NOLOCK) WHERE Id = @StudentId",
                    new { requestData.StudentId });

                if (student == null)
                {
                    return Failure("Student not found.");
                }

                var user = await _sqlQueryHelper.GetSingleAsync<UserModel>(
                    "SELECT * FROM Users WITH (NOLOCK) WHERE UserId = @UserId",
                    new { student.UserId });

                if (user == null)
                {
                    return Failure("Student user not found.");
                }

                var existingApplication = await _sqlQueryHelper.GetSingleAsync<JobApplicationModel>(
                    @"SELECT TOP 1 * FROM JobApplications WITH (NOLOCK)
WHERE JobId = @JobId AND StudentId = @StudentId",
                    new { JobId = jobId, StudentId = requestData.StudentId });

                if (existingApplication != null)
                {
                    return Failure("You have already applied for this job.");
                }

                JobApplicationModel application = new()
                {
                    JobId = jobId,
                    CompanyId = job.CompanyId,
                    StudentId = student.Id,
                    StudentUserId = student.UserId,
                    StudentName = string.Join(" ", new[] { student.FirstName, student.MiddleName, student.LastName }
                        .Where(x => !string.IsNullOrWhiteSpace(x))),
                    StudentEmail = user.Email,
                    StudentPhone = user.MobileNo,
                    ResumeFilePath = student.ResumeFilePath,
                    Status = "applied",
                    AppliedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                string insertSql = @"
INSERT INTO JobApplications
(
    JobId, CompanyId, StudentId, StudentUserId, StudentName, StudentEmail,
    StudentPhone, ResumeFilePath, Status, AppliedAt, UpdatedAt
)
VALUES
(
    @JobId, @CompanyId, @StudentId, @StudentUserId, @StudentName, @StudentEmail,
    @StudentPhone, @ResumeFilePath, @Status, @AppliedAt, @UpdatedAt
);
SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

                long? applicationId = await _sqlQueryHelper.GetSingleAsync<long>(insertSql, application);
                if (applicationId == null)
                {
                    return Failure("Failed to apply for the job.");
                }

                application.ApplicationId = applicationId.Value;
                return Success("Job application submitted successfully.", application);
            }
            catch (Exception ex)
            {
                return Failure(ex.Message, ex);
            }
        }

        [HttpGet("applications/company/{companyId:long}")]
        public async Task<ActionResult<ResponseModel>> GetCompanyApplications(long companyId)
        {
            try
            {
                await EnsureJobApplicationsTableAsync();

                if (companyId <= 0)
                {
                    return Failure("Company is required.");
                }

                const string sql = @"
SELECT
    ja.*,
    j.JobTitle
FROM JobApplications ja
INNER JOIN Jobs j ON ja.JobId = j.JobId
WHERE ja.CompanyId = @CompanyId
ORDER BY ja.AppliedAt DESC";

                List<CompanyJobApplicationListItemModel> applications =
                    await _sqlQueryHelper.GetListAsync<CompanyJobApplicationListItemModel>(sql, new { CompanyId = companyId });

                return Success("Applications retrieved successfully.", applications);
            }
            catch (Exception ex)
            {
                return Failure(ex.Message, ex);
            }
        }

        [HttpGet("applications/{applicationId:long}")]
        public async Task<ActionResult<ResponseModel>> GetApplicationDetails(long applicationId, [FromQuery] long companyId)
        {
            try
            {
                await EnsureJobApplicationsTableAsync();

                if (companyId <= 0)
                {
                    return Failure("Company is required.");
                }

                const string sql = @"
SELECT
    ja.*,
    j.JobTitle,
    j.JobType,
    j.WorkMode,
    j.Location,
    j.Qualifications,
    j.RequiredSkills,
    s.FirstName AS StudentFirstName,
    s.MiddleName AS StudentMiddleName,
    s.LastName AS StudentLastName,
    s.Department,
    s.PassingYear,
    s.CGPA,
    s.Skills,
    s.ResumeFilePath AS ResumeUrl
FROM JobApplications ja
INNER JOIN Jobs j ON ja.JobId = j.JobId
INNER JOIN Students s ON ja.StudentId = s.Id
WHERE ja.ApplicationId = @ApplicationId AND ja.CompanyId = @CompanyId";

                CompanyJobApplicationDetailModel? application =
                    await _sqlQueryHelper.GetSingleAsync<CompanyJobApplicationDetailModel>(
                        sql,
                        new { ApplicationId = applicationId, CompanyId = companyId });

                if (application == null)
                {
                    return Failure("Application not found.");
                }

                return Success("Application details retrieved successfully.", application);
            }
            catch (Exception ex)
            {
                return Failure(ex.Message, ex);
            }
        }

        [HttpPatch("applications/{applicationId:long}/schedule-interview")]
        public async Task<ActionResult<ResponseModel>> ScheduleInterview(long applicationId, [FromBody] ScheduleInterviewRequestModel requestData)
        {
            try
            {
                await EnsureJobApplicationsTableAsync();

                if (requestData == null || requestData.CompanyId <= 0)
                {
                    return Failure("Company is required.");
                }

                if (requestData.InterviewScheduledAt == default)
                {
                    return Failure("Interview date and time are required.");
                }

                JobApplicationModel? application = await GetOwnedApplicationAsync(applicationId, requestData.CompanyId);
                if (application == null)
                {
                    return Failure("Application not found.");
                }

                if (string.Equals(application.Status, "accepted", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(application.Status, "rejected", StringComparison.OrdinalIgnoreCase))
                {
                    return Failure("Interview cannot be scheduled after the application is finalized.");
                }

                int rowsAffected = await _sqlQueryHelper.ExecuteAsync(@"
UPDATE JobApplications
SET Status = @Status,
    InterviewScheduledAt = @InterviewScheduledAt,
    InterviewMode = @InterviewMode,
    InterviewLocation = @InterviewLocation,
    InterviewNotes = @InterviewNotes,
    UpdatedAt = @UpdatedAt
WHERE ApplicationId = @ApplicationId AND CompanyId = @CompanyId", new
                {
                    ApplicationId = applicationId,
                    requestData.CompanyId,
                    Status = "interview_scheduled",
                    InterviewScheduledAt = requestData.InterviewScheduledAt,
                    InterviewMode = requestData.InterviewMode,
                    InterviewLocation = requestData.InterviewLocation,
                    InterviewNotes = requestData.InterviewNotes,
                    UpdatedAt = DateTime.UtcNow
                });

                if (rowsAffected == 0)
                {
                    return Failure("Failed to schedule interview.");
                }

                CompanyJobApplicationDetailModel? updatedApplication = await GetApplicationDetailAsync(applicationId, requestData.CompanyId);
                return Success("Interview scheduled successfully.", updatedApplication);
            }
            catch (Exception ex)
            {
                return Failure(ex.Message, ex);
            }
        }

        [HttpPatch("applications/{applicationId:long}/status")]
        public async Task<ActionResult<ResponseModel>> UpdateApplicationStatus(long applicationId, [FromBody] JobApplicationStatusUpdateModel requestData)
        {
            try
            {
                await EnsureJobApplicationsTableAsync();

                if (requestData == null || requestData.CompanyId <= 0)
                {
                    return Failure("Company is required.");
                }

                string normalizedStatus = NormalizeApplicationStatus(requestData.Status);
                if (normalizedStatus != "accepted" && normalizedStatus != "rejected")
                {
                    return Failure("Only accepted or rejected are allowed.");
                }

                if (normalizedStatus == "accepted" && !requestData.JoiningDate.HasValue)
                {
                    return Failure("Joining date is required when accepting a student.");
                }

                JobApplicationModel? application = await GetOwnedApplicationAsync(applicationId, requestData.CompanyId);
                if (application == null)
                {
                    return Failure("Application not found.");
                }

                if (!application.InterviewScheduledAt.HasValue)
                {
                    return Failure("Schedule an interview before updating the final status.");
                }

                if (application.InterviewScheduledAt.Value > DateTime.UtcNow)
                {
                    return Failure("Final decision can only be made after the scheduled interview time.");
                }

                DateTime updatedAt = DateTime.UtcNow;
                int rowsAffected = await _sqlQueryHelper.ExecuteAsync(@"
DECLARE @ExistingStatus NVARCHAR(30);
DECLARE @JobId BIGINT;

SELECT TOP 1
    @ExistingStatus = Status,
    @JobId = JobId
FROM JobApplications WITH (UPDLOCK, HOLDLOCK)
WHERE ApplicationId = @ApplicationId AND CompanyId = @CompanyId;

IF @ExistingStatus IS NULL
BEGIN
    THROW 50000, 'Application not found.', 1;
END

IF @Status = 'accepted' AND LOWER(ISNULL(@ExistingStatus, '')) = 'accepted'
BEGIN
    THROW 50001, 'Student has already been accepted for this job.', 1;
END

BEGIN TRANSACTION;

IF @Status = 'accepted' AND LOWER(ISNULL(@ExistingStatus, '')) <> 'accepted'
BEGIN
    UPDATE Jobs
    SET Openings = Openings - 1,
        UpdatedAt = @UpdatedAt
    WHERE JobId = @JobId
      AND Openings > 0;

    IF @@ROWCOUNT = 0
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50002, 'No job openings are available for this job.', 1;
    END
END

UPDATE JobApplications
SET Status = @Status,
    DecisionAt = @DecisionAt,
    JoiningDate = @JoiningDate,
    UpdatedAt = @UpdatedAt
WHERE ApplicationId = @ApplicationId AND CompanyId = @CompanyId;

IF @@ROWCOUNT = 0
BEGIN
    ROLLBACK TRANSACTION;
    THROW 50003, 'Failed to update application status.', 1;
END

COMMIT TRANSACTION;", new
                {
                    ApplicationId = applicationId,
                    requestData.CompanyId,
                    Status = normalizedStatus,
                    DecisionAt = updatedAt,
                    JoiningDate = normalizedStatus == "accepted" ? requestData.JoiningDate?.Date : null,
                    UpdatedAt = updatedAt
                });

                if (rowsAffected == 0)
                {
                    return Failure("Failed to update application status.");
                }

                CompanyJobApplicationDetailModel? updatedApplication = await GetApplicationDetailAsync(applicationId, requestData.CompanyId);
                return Success("Application status updated successfully.", updatedApplication);
            }
            catch (Exception ex)
            {
                return Failure(ex.Message, ex);
            }
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel>> Post([FromForm] JobCreateDTO requestData)
        {
            try
            {
                if (requestData == null || string.IsNullOrWhiteSpace(requestData.Data))
                {
                    return Failure("Job data is required.");
                }

                JobModel? job = DeserializeJob(requestData.Data);
                if (job == null)
                {
                    return Failure("Invalid job data.");
                }

                string? validationMessage = await ValidateJobAsync(job, false);
                if (!string.IsNullOrEmpty(validationMessage))
                {
                    return Failure(validationMessage);
                }

                job.Status = NormalizeStatus(job.Status);
                job.CreatedAt = DateTime.UtcNow;
                
                job.UpdatedAt = DateTime.UtcNow;

                string sql = @"
INSERT INTO Jobs
(
    CompanyId, JobTitle, Department, JobType, WorkMode, Location,
    ExperienceMin, ExperienceMax, SalaryMin, SalaryMax, Openings,
    Responsibilities, RequiredSkills, PreferredSkills, Qualifications, Benefits,
    Status, ExpiryDate, CreatedBy, CreatedAt, UpdatedAt
)
VALUES
(
    @CompanyId, @JobTitle, @Department, @JobType, @WorkMode, @Location,
    @ExperienceMin, @ExperienceMax, @SalaryMin, @SalaryMax, @Openings,
    @Responsibilities, @RequiredSkills, @PreferredSkills, @Qualifications, @Benefits,
    @Status, @ExpiryDate, @CreatedBy, @CreatedAt, @UpdatedAt
);
SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

                long? jobId = await _sqlQueryHelper.GetSingleAsync<long>(sql, job);
                if (jobId == null)
                {
                    return Failure("Failed to save job.");
                }

                job.JobId = jobId.Value;
                return Success("Job saved successfully.", job);
            }
            catch (Exception ex)
            {
                return Failure(ex.Message, ex);
            }
        }

        [HttpPut("{jobId:long}")]
        public async Task<ActionResult<ResponseModel>> Put(long jobId, [FromForm] JobCreateDTO requestData)
        {
            try
            {
                if (requestData == null || string.IsNullOrWhiteSpace(requestData.Data))
                {
                    return Failure("Job data is required.");
                }

                JobModel? job = DeserializeJob(requestData.Data);
                if (job == null)
                {
                    return Failure("Invalid job data.");
                }

                job.JobId = jobId;

                string? validationMessage = await ValidateJobAsync(job, true);
                if (!string.IsNullOrEmpty(validationMessage))
                {
                    return Failure(validationMessage);
                }

                job.Status = NormalizeStatus(job.Status);
                job.UpdatedAt = DateTime.UtcNow;

                string sql = @"
UPDATE Jobs
SET CompanyId = @CompanyId,
    JobTitle = @JobTitle,
    Department = @Department,
    JobType = @JobType,
    WorkMode = @WorkMode,
    Location = @Location,
    ExperienceMin = @ExperienceMin,
    ExperienceMax = @ExperienceMax,
    SalaryMin = @SalaryMin,
    SalaryMax = @SalaryMax,
    Openings = @Openings,
    Responsibilities = @Responsibilities,
    RequiredSkills = @RequiredSkills,
    PreferredSkills = @PreferredSkills,
    Qualifications = @Qualifications,
    Benefits = @Benefits,
    Status = @Status,
    ExpiryDate = @ExpiryDate,
    CreatedBy = @CreatedBy,
    UpdatedAt = @UpdatedAt
WHERE JobId = @JobId";

                int rowsAffected = await _sqlQueryHelper.ExecuteAsync(sql, job);
                if (rowsAffected == 0)
                {
                    return Failure("Job not found.");
                }

                return Success("Job updated successfully.", job);
            }
            catch (Exception ex)
            {
                return Failure(ex.Message, ex);
            }
        }

        [HttpPatch("{jobId:long}/status")]
        public async Task<ActionResult<ResponseModel>> UpdateStatus(long jobId, [FromBody] JobStatusUpdateModel requestData)
        {
            try
            {
                string status = NormalizeStatus(requestData?.Status);
                if (!JobStatus.AllowedStatuses.Contains(status))
                {
                    return Failure("Invalid job status.");
                }

                int rowsAffected = await _sqlQueryHelper.ExecuteAsync(@"
UPDATE Jobs
SET Status = @Status,
    UpdatedAt = @UpdatedAt
WHERE JobId = @JobId", new
                {
                    JobId = jobId,
                    Status = status,
                    UpdatedAt = DateTime.UtcNow
                });

                if (rowsAffected == 0)
                {
                    return Failure("Job not found.");
                }

                return Success("Job status updated successfully.");
            }
            catch (Exception ex)
            {
                return Failure(ex.Message, ex);
            }
        }

        [HttpPatch("{jobId:long}/expire")]
        public async Task<ActionResult<ResponseModel>> ExpireJob(long jobId, [FromBody] JobExpireModel? requestData)
        {
            try
            {
                DateTime expiryDate = (requestData?.ExpiryDate ?? DateTime.UtcNow).Date;

                int rowsAffected = await _sqlQueryHelper.ExecuteAsync(@"
UPDATE Jobs
SET Status = @Status,
    ExpiryDate = @ExpiryDate,
    UpdatedAt = @UpdatedAt
WHERE JobId = @JobId", new
                {
                    JobId = jobId,
                    Status = JobStatus.Expired,
                    ExpiryDate = expiryDate,
                    UpdatedAt = DateTime.UtcNow
                });

                if (rowsAffected == 0)
                {
                    return Failure("Job not found.");
                }

                return Success("Job expired successfully.");
            }
            catch (Exception ex)
            {
                return Failure(ex.Message, ex);
            }
        }

        [HttpPatch("{jobId:long}/openings-expiry")]
        public async Task<ActionResult<ResponseModel>> UpdateOpeningsAndExpiry(long jobId, [FromBody] JobOpeningsExpiryUpdateModel requestData)
        {
            try
            {
                if (requestData == null)
                {
                    return Failure("Request data is required.");
                }

                if (requestData.Openings < 0)
                {
                    return Failure("Openings cannot be negative.");
                }

                if (requestData.ExpiryDate.HasValue && requestData.ExpiryDate.Value.Date < DateTime.UtcNow.Date)
                {
                    return Failure("Expiry date cannot be in the past.");
                }

                int rowsAffected = await _sqlQueryHelper.ExecuteAsync(@"
UPDATE Jobs
SET Openings = @Openings,
    ExpiryDate = @ExpiryDate,
    UpdatedAt = @UpdatedAt
WHERE JobId = @JobId", new
                {
                    JobId = jobId,
                    requestData.Openings,
                    requestData.ExpiryDate,
                    UpdatedAt = DateTime.UtcNow
                });

                if (rowsAffected == 0)
                {
                    return Failure("Job not found.");
                }

                return Success("Job openings and expiry date updated successfully.");
            }
            catch (Exception ex)
            {
                return Failure(ex.Message, ex);
            }
        }

        [HttpDelete("{jobId:long}")]
        public async Task<ActionResult<ResponseModel>> Delete(long jobId)
        {
            try
            {
                int rowsAffected = await _sqlQueryHelper.ExecuteAsync(
                    "DELETE FROM Jobs WHERE JobId = @JobId",
                    new { JobId = jobId });

                if (rowsAffected == 0)
                {
                    return Failure("Job not found.");
                }

                return Success("Job deleted successfully.");
            }
            catch (Exception ex)
            {
                return Failure(ex.Message, ex);
            }
        }

        private static JobModel? DeserializeJob(string data)
        {
            return JsonSerializer.Deserialize<JobModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            });
        }

        private async Task<string?> ValidateJobAsync(JobModel job, bool isUpdate)
        {
            if (isUpdate)
            {
                JobModel? existingJob = await _sqlQueryHelper.GetSingleAsync<JobModel>(
                    "SELECT * FROM Jobs WITH (NOLOCK) WHERE JobId = @JobId",
                    new { job.JobId });

                if (existingJob == null)
                {
                    return "Job not found.";
                }
            }

            if (job.CompanyId <= 0) return "Company is required.";
            if (job.CreatedBy <= 0) return "Created by is required.";
            if (string.IsNullOrWhiteSpace(job.JobTitle)) return "Job title is required.";
            if (string.IsNullOrWhiteSpace(job.JobType)) return "Job type is required.";
            if (string.IsNullOrWhiteSpace(job.WorkMode)) return "Work mode is required.";
            if (string.IsNullOrWhiteSpace(job.Location)) return "Location is required.";
            if (job.ExperienceMin < 0 || job.ExperienceMax < 0) return "Experience cannot be negative.";
            if (job.ExperienceMax < job.ExperienceMin) return "Maximum experience must be greater than or equal to minimum experience.";
            if (job.SalaryMin < 0 || job.SalaryMax < 0) return "Salary cannot be negative.";
            if (job.SalaryMin.HasValue && job.SalaryMax.HasValue && job.SalaryMax < job.SalaryMin) return "Maximum salary must be greater than or equal to minimum salary.";
            if (job.Openings < 1) return "Openings must be at least 1.";
            if (job.ExpiryDate.HasValue && job.ExpiryDate.Value.Date < DateTime.UtcNow.Date) return "Expiry date cannot be in the past.";

            string status = NormalizeStatus(job.Status);
            if (!JobStatus.AllowedStatuses.Contains(status)) return "Invalid job status.";

            CompanyModel? company = await _sqlQueryHelper.GetSingleAsync<CompanyModel>(
                "SELECT * FROM Companies WITH (NOLOCK) WHERE Id = @Id",
                new { Id = job.CompanyId });
            if (company == null) return "Company not found.";

            UserModel? creator = await _sqlQueryHelper.GetSingleAsync<UserModel>(
                "SELECT * FROM Users WITH (NOLOCK) WHERE UserId = @UserId",
                new { UserId = job.CreatedBy });
            if (creator == null) return "Creator not found.";

            return null;
        }

        private static string NormalizeStatus(string? status)
        {
            return string.IsNullOrWhiteSpace(status) ? JobStatus.Draft : status.Trim().ToLowerInvariant();
        }

        private static string NormalizeApplicationStatus(string? status)
        {
            return string.IsNullOrWhiteSpace(status) ? "applied" : status.Trim().ToLowerInvariant();
        }

        private async Task<JobApplicationModel?> GetOwnedApplicationAsync(long applicationId, long companyId)
        {
            return await _sqlQueryHelper.GetSingleAsync<JobApplicationModel>(
                @"SELECT TOP 1 *
FROM JobApplications WITH (NOLOCK)
WHERE ApplicationId = @ApplicationId AND CompanyId = @CompanyId",
                new { ApplicationId = applicationId, CompanyId = companyId });
        }

        private async Task<CompanyJobApplicationDetailModel?> GetApplicationDetailAsync(long applicationId, long companyId)
        {
            const string sql = @"
SELECT
    ja.*,
    j.JobTitle,
    j.JobType,
    j.WorkMode,
    j.Location,
    j.Qualifications,
    j.RequiredSkills,
    s.FirstName AS StudentFirstName,
    s.MiddleName AS StudentMiddleName,
    s.LastName AS StudentLastName,
    s.Department,
    s.PassingYear,
    s.CGPA,
    s.Skills,
    s.ResumeFilePath AS ResumeUrl
FROM JobApplications ja
INNER JOIN Jobs j ON ja.JobId = j.JobId
INNER JOIN Students s ON ja.StudentId = s.Id
WHERE ja.ApplicationId = @ApplicationId AND ja.CompanyId = @CompanyId";

            return await _sqlQueryHelper.GetSingleAsync<CompanyJobApplicationDetailModel>(
                sql,
                new { ApplicationId = applicationId, CompanyId = companyId });
        }

        private async Task EnsureJobApplicationsTableAsync()
        {
            const string sql = @"
IF OBJECT_ID('dbo.JobApplications', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[JobApplications] (
        ApplicationId BIGINT IDENTITY(1,1) PRIMARY KEY,
        JobId BIGINT NOT NULL,
        CompanyId BIGINT NOT NULL,
        StudentId BIGINT NOT NULL,
        StudentUserId BIGINT NOT NULL,
        StudentName NVARCHAR(250) NOT NULL,
        StudentEmail NVARCHAR(255) NOT NULL,
        StudentPhone NVARCHAR(20) NULL,
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
