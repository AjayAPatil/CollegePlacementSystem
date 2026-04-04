namespace API.Model
{
    public class StudentModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }

        //Personal Info
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string BloodGroup { get; set; } = string.Empty;
        //Student Info
        public string EnrollmentNo { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int PassingYear { get; set; }
        public decimal CGPA { get; set; }
        public string? ResumeFilePath { get; set; }
        public string? Skills { get; set; }
        public string? SelectedCompanyName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // Navigation
        public UserModel? User { get; set; }
    }

    public class StudentDashboardModel
    {
        public int ProfileCompletionPercentage { get; set; }
        public int AppliedJobsCount { get; set; }
        public int UpcomingDrivesCount { get; set; }
        public int ShortlistedCount { get; set; }
        public List<StudentDashboardRecentApplicationModel> RecentApplications { get; set; } = [];
        public List<StudentDashboardChartItemModel> ApplicationStatusChart { get; set; } = [];
        public List<StudentDashboardChartItemModel> SkillDemandChart { get; set; } = [];
    }

    public class StudentDashboardRecentApplicationModel
    {
        public long ApplicationId { get; set; }
        public long JobId { get; set; }
        public string Company { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
    }

    public class StudentDashboardChartItemModel
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
