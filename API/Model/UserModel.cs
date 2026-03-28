namespace API.Model
{
    public class UserModel
    {
        //User Info
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Admin, Student, Company
        public string Status { get; set; } = "Active"; // Active, Pending, Rejected
        public string? ProfileImagePath { get; set; }

        //Contact Info
        public string Email { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string StreetAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public long PinCode { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public StudentModel? Student { get; set; }
        public CompanyModel? Company { get; set; }
    }
    public class UserCreateDTO
    {
        public IFormFile? Resume { get; set; }
        public IFormFile? ProfilePhoto { get; set; }
        public string? Data { get; set; }
    }

    public class ChangePasswordRequest
    {
        public long UserId { get; set; }
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string VerifyNewPassword { get; set; } = string.Empty;
    }
}
