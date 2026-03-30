using API.Common;
using API.Constants;
using API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class CompanyController : BaseApiController
    {
        private readonly ISqlQueryHelper _sqlQueryHelper;
        public CompanyController(ISqlQueryHelper sqlQueryHelper)
        {
            _sqlQueryHelper = sqlQueryHelper;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel>> Get()
        {
            try
            {
                string sql = "select * from Companies c with(nolock)";
                List<CompanyModel> companyList = await _sqlQueryHelper.GetListAsync<CompanyModel>(sql);

                if (companyList.Count == 0)
                {
                    return Success("Companies retrieved successfully.", companyList);
                }

                sql = "select * from Users u with(nolock) where Email in ("
                    + string.Join(",", companyList.Select(c => $"'{c.ContactEmail ?? c.CompanyName.Replace(" ", "")[..10]}'").ToList())
                    + ")";
                List<UserModel> userList = await _sqlQueryHelper.GetListAsync<UserModel>(sql);
                companyList = companyList.Select(company =>
                {
                    company.Users = userList.Where(u => u.UserName == (company.ContactEmail ?? company.CompanyName.Replace(" ", "")[..10])).ToList();
                    return company;
                })
                .ToList();

                return Success("Companies retrieved successfully.", companyList);
            }
            catch (Exception ex)
            {
                return Failure($"Error - {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel>> Post([FromForm] CompanyCreateDTO requestData)
        {
            try
            {
                if (requestData == null)
                {
                    return Failure("Request data is null");
                }

                if (string.IsNullOrEmpty(requestData.Data))
                {
                    return Failure("Request data is null");
                }

                CompanyModel? company = System.Text.Json.JsonSerializer.Deserialize<CompanyModel>(requestData.Data, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                });
                if (company == null)
                {
                    return Failure("Invalid company data.");
                }

                if (requestData.CompanyLogo != null)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(requestData.CompanyLogo.FileName);

                    string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "CompanyLogos");

                    if (!Directory.Exists(uploadFolder))
                    {
                        _ = Directory.CreateDirectory(uploadFolder);
                    }

                    string path = Path.Combine(uploadFolder, fileName);

                    using (FileStream stream = new(path, FileMode.Create))
                    {
                        await requestData.CompanyLogo.CopyToAsync(stream);
                    }

                    company.LogoUrl = path;
                }

                string sql = "select * from Companies with(nolock) where ContactEmail = @ContactEmail";
                List<CompanyModel> companyList = await _sqlQueryHelper.GetListAsync<CompanyModel>(sql, new { company.ContactEmail });

                if (companyList.Any())
                {
                    return Failure("Company already registered!");
                }
                sql = "select * from Users with(nolock) where Email = @ContactEmail";
                List<UserModel> userList = await _sqlQueryHelper.GetListAsync<UserModel>(sql, new { company.ContactEmail });

                if (userList.Any())
                {
                    return Failure("Email Id Registered in Users!");
                }

                UserModel user = new()
                {
                    PasswordHash = "Pass@123",
                    CreatedAt = DateTime.UtcNow,
                    ProfileImagePath = company.LogoUrl,
                    Email = company.ContactEmail ?? company.CompanyName.Replace(" ", "")[..10],
                    IsDeleted = false,
                    MobileNo = company.ContactPhone ?? string.Empty,
                    Role = UserRole.Company,
                    UserName = string.Empty,
                    UpdatedAt = DateTime.UtcNow,
                    Status = UserStatus.Active,
                };
                user.UserName = user.Email;
                // Insert User
                string userSql = @"
INSERT INTO Users (UserName, PasswordHash, Role, Status, ProfileImagePath, Email, MobileNo, StreetAddress, City, District, State, Country, PinCode, IsDeleted, CreatedAt, UpdatedAt)
VALUES (@UserName, @PasswordHash, @Role, @Status, @ProfileImagePath, @Email, @MobileNo, @StreetAddress, @City, @District, @State, @Country, @PinCode, @IsDeleted, @CreatedAt, @UpdatedAt);
SELECT CAST(SCOPE_IDENTITY() as bigint);
";

                long? newUserId = await _sqlQueryHelper.GetSingleAsync<long>(userSql, user);
                if (newUserId == null)
                {
                    return Failure("Failed to create user.", user);
                }

                company.UserId = (long)newUserId;

                string companySql = @"
Insert into Companies (UserId, CompanyName, Industry, CompanySize, Website, Description, CreatedAt, Location, HRName, ContactEmail, ContactPhone, FoundedYear, LogoUrl)
values (@UserId, @CompanyName, @Industry, @CompanySize, @Website, @Description, @CreatedAt, @Location, @HRName, @ContactEmail, @ContactPhone, @FoundedYear, @LogoUrl)
";
                _ = await _sqlQueryHelper.ExecuteAsync(companySql, company);

                company.Users = [user];

                return Success("Company Resgistration Completed!", company);
            }
            catch (Exception ex)
            {
                return Failure($"Error - {ex.Message}");
            }
        }
    }
}
