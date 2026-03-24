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
}
