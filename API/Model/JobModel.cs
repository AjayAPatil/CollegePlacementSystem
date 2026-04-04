namespace API.Model
{
    public class JobModel
    {
        public long JobId { get; set; }
        public long CompanyId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string JobType { get; set; } = string.Empty;
        public string WorkMode { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int ExperienceMin { get; set; }
        public int ExperienceMax { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public int Openings { get; set; } = 1;
        public string? Responsibilities { get; set; }
        public string? RequiredSkills { get; set; }
        public string? PreferredSkills { get; set; }
        public string? Qualifications { get; set; }
        public string? Benefits { get; set; }
        public string Status { get; set; } = "draft";
        public DateTime? ExpiryDate { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public CompanyModel? Company { get; set; }
        public UserModel? Creator { get; set; }
    }

    public class JobCreateDTO
    {
        public string? Data { get; set; }
    }

    public class JobStatusUpdateModel
    {
        public string Status { get; set; } = string.Empty;
    }

    public class JobExpireModel
    {
        public DateTime? ExpiryDate { get; set; }
    }

    public class JobOpeningsExpiryUpdateModel
    {
        public int Openings { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class JobViewModel : JobModel
    {
        public string CompanyName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public bool IsApplied { get; set; }
    }

    public class JobDetailViewModel : JobViewModel
    {
        public string? CompanyWebsite { get; set; }
        public string? CompanyDescription { get; set; }
        public string? CompanyIndustry { get; set; }
        public string? CompanyLocation { get; set; }
        public string? CompanyHrName { get; set; }
        public string? CompanyContactEmail { get; set; }
        public string? CompanyContactPhone { get; set; }
        public int? CompanyFoundedYear { get; set; }
        public int? CompanySize { get; set; }
    }

    public class JobApplicationModel
    {
        public long ApplicationId { get; set; }
        public long JobId { get; set; }
        public long CompanyId { get; set; }
        public long StudentId { get; set; }
        public long StudentUserId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string? StudentPhone { get; set; }
        public string? ResumeFilePath { get; set; }
        public string Status { get; set; } = "applied";
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public DateTime? InterviewScheduledAt { get; set; }
        public string? InterviewMode { get; set; }
        public string? InterviewLocation { get; set; }
        public string? InterviewNotes { get; set; }
        public DateTime? DecisionAt { get; set; }
        public DateTime? JoiningDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CompanyJobApplicationListItemModel : JobApplicationModel
    {
        public string JobTitle { get; set; } = string.Empty;
    }

    public class StudentJobApplicationListItemModel : JobApplicationModel
    {
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyLogoUrl { get; set; }
        public string? CompanyLocation { get; set; }
        public string? WorkMode { get; set; }
    }

    public class CompanyJobApplicationDetailModel : CompanyJobApplicationListItemModel
    {
        public string StudentFirstName { get; set; } = string.Empty;
        public string StudentMiddleName { get; set; } = string.Empty;
        public string StudentLastName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public int PassingYear { get; set; }
        public decimal CGPA { get; set; }
        public string? Skills { get; set; }
        public string? ResumeUrl { get; set; }
        public string JobType { get; set; } = string.Empty;
        public string WorkMode { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? Qualifications { get; set; }
        public string? RequiredSkills { get; set; }
    }

    public class JobApplyRequestModel
    {
        public long StudentId { get; set; }
    }

    public class CompanyJobApplicationQueryModel
    {
        public long CompanyId { get; set; }
    }

    public class ScheduleInterviewRequestModel : CompanyJobApplicationQueryModel
    {
        public DateTime InterviewScheduledAt { get; set; }
        public string? InterviewMode { get; set; }
        public string? InterviewLocation { get; set; }
        public string? InterviewNotes { get; set; }
    }

    public class JobApplicationStatusUpdateModel : CompanyJobApplicationQueryModel
    {
        public string Status { get; set; } = string.Empty;
        public DateTime? JoiningDate { get; set; }
    }
}
