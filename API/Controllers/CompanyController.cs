using API.Common;
using API.Constants;
using API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ISqlQueryHelper _sqlQueryHelper;
        public CompanyController(ISqlQueryHelper sqlQueryHelper)
        {
            _sqlQueryHelper = sqlQueryHelper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompanyModel>>> Get()
        {
            string sql = "select * from Companies c with(nolock)";
            List<CompanyModel> companyList = await _sqlQueryHelper.GetListAsync<CompanyModel>(sql);
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

            return Ok(companyList);
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel>> Post([FromForm] CompanyCreateDTO requestData)
        {
            try
            {
                if (requestData == null)
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "Request data is null",
                    });
                }

                if (string.IsNullOrEmpty(requestData.Data))
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "Request data is null",
                    });
                }

                CompanyModel? company = System.Text.Json.JsonSerializer.Deserialize<CompanyModel>(requestData.Data, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                });
                if (company == null)
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "Invalid company data.",
                    });
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
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "Company already registered!",
                    });
                }
                sql = "select * from Users with(nolock) where Email = @ContactEmail";
                List<UserModel> userList = await _sqlQueryHelper.GetListAsync<UserModel>(sql, new { company.ContactEmail });

                if (userList.Any())
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "Email Id Registered in Users!",
                    });
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
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "Failed to create user.",
                        Data = user
                    });
                }

                company.UserId = (long)newUserId;

                string companySql = @"
Insert into Companies (UserId, CompanyName, Industry, CompanySize, Website, Description, CreatedAt, Location, HRName, ContactEmail, ContactPhone, FoundedYear, LogoUrl)
values (@UserId, @CompanyName, @Industry, @CompanySize, @Website, @Description, @CreatedAt, @Location, @HRName, @ContactEmail, @ContactPhone, @FoundedYear, @LogoUrl)
";
                _ = await _sqlQueryHelper.ExecuteAsync(companySql, company);

                company.Users = [user];

                return Ok(new ResponseModel
                {
                    Status = ResponseStatus.Success,
                    Message = "Company Resgistration Completed!",
                    Data = company
                });
            }
            catch (Exception ex)
            {
                return Ok(new ResponseModel
                {
                    Status = ResponseStatus.Failure,
                    Message = ex.Message,
                    Data = ex
                });
            }
        }
    }
}
