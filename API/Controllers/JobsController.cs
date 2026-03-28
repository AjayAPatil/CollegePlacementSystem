using API.Common;
using API.Constants;
using API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly ISqlQueryHelper _sqlQueryHelper;

        public JobsController(ISqlQueryHelper sqlQueryHelper)
        {
            _sqlQueryHelper = sqlQueryHelper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobModel>>> Get()
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

            return Ok(jobList);
        }

        [HttpGet("{page}/{pageSize}")]
        public async Task<ActionResult<ResponseModel>> Get(int page = 1, int pageSize = 10)
        {
            try
            {
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

            u.UserName AS CreatorName

        FROM Jobs j
        LEFT JOIN Companies c ON j.CompanyId = c.Id
        LEFT JOIN Users u ON j.CreatedBy = u.UserId
        WHERE j.Status = 'published'

        ORDER BY j.CreatedAt DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                int totalCount = await _sqlQueryHelper.GetSingleAsync<int>(countSql);
                var jobs = await _sqlQueryHelper.GetListAsync<JobViewModel>(
                    sql,
                    new { Offset = offset, PageSize = pageSize });

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

        private ActionResult<ResponseModel> Success(string message, object? data = null)
        {
            return Ok(new ResponseModel
            {
                Status = ResponseStatus.Success,
                Message = message,
                Data = data
            });
        }

        private ActionResult<ResponseModel> Failure(string message, object? data = null)
        {
            return Ok(new ResponseModel
            {
                Status = ResponseStatus.Failure,
                Message = message,
                Data = data
            });
        }
    }
}
