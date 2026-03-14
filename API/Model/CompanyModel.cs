namespace API.Model
{
    public class CompanyModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string? Description { get; set; }
        public string? Industry { get; set; }
        public string? Location { get; set; }
        public string? HRName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public int? FoundedYear { get; set; }
        public int? CompanySize { get; set; }
        public string? LogoUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public List<UserModel>? Users { get; set; }
    }
    public class CompanyCreateDTO
    {
        public IFormFile? CompanyLogo { get; set; }
        public string? Data { get; set; }
    }
}
