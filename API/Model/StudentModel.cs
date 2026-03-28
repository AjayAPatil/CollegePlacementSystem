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
}
