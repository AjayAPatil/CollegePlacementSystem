using API.Common;
using API.Constants;
using API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ISqlQueryHelper _sqlQueryHelper;

        public UserController(ISqlQueryHelper sqlQueryHelper)
        {
            _sqlQueryHelper = sqlQueryHelper;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserModel>>> Get()
        {
            string sql = "SELECT * FROM Users With(nolock)";
            List<UserModel>? userList = await _sqlQueryHelper.GetListAsync<UserModel>(sql);
            return userList == null || userList.Count == 0 ? (ActionResult<List<UserModel>>)NotFound() : (ActionResult<List<UserModel>>)Ok(userList);
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<UserModel>> Get(long userId)
        {
            string sql = "SELECT * FROM Users with(nolock) WHERE UserId = @UserId";
            UserModel? user = await _sqlQueryHelper.GetSingleAsync<UserModel>(sql, new { UserId = userId });
            return user == null ? (ActionResult<UserModel>)NotFound() : (ActionResult<UserModel>)Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] UserCreateDTO reqestData)
        {
            try
            {
                if (reqestData == null)
                    return BadRequest("User data is required.");
                if (string.IsNullOrEmpty(reqestData.Data))
                    return BadRequest("User data is required.");
                UserModel? user = System.Text.Json.JsonSerializer.Deserialize<UserModel>(reqestData.Data, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                });
                if (user == null)
                    return BadRequest("Invalid user data.");

                if (user.UserName == null || user.Email == null || user.PasswordHash == null || user.Role == null || user.Status == null)
                {
                    return BadRequest("All fields are required.");
                }

                if (user.Role != UserRole.Admin.ToString() && user.Role == UserRole.Student.ToString() && user.Role == UserRole.Company.ToString())
                {
                    return BadRequest("Invalid role specified.");
                }

                if (user.Status != UserStatus.Active.ToString() && user.Status != UserStatus.Inactive.ToString() && user.Status != UserStatus.Pending && user.Status != UserStatus.Rejected)
                {
                    return BadRequest("Invalid status specified.");
                }

                if (user.Role == UserRole.Student.ToString()
                    && (user.Student == null || string.IsNullOrEmpty(user.Student.FirstName) || string.IsNullOrEmpty(user.Student.LastName) || string.IsNullOrEmpty(user.Student.EnrollmentNo)))
                {
                    return BadRequest("Student details are required for student role.");
                }
                if (user.Role == UserRole.Company.ToString() && user.Company == null)
                {
                    return BadRequest("Company details are required for company role.");
                }

                string profilePath = "";
                string resumePath = "";

                if (reqestData.ProfilePhoto != null)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(reqestData.ProfilePhoto.FileName);

                    var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    string path = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await reqestData.ProfilePhoto.CopyToAsync(stream);
                    }

                    profilePath = fileName;
                }

                if (reqestData.Resume != null)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(reqestData.Resume.FileName);

                    var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    string path = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await reqestData.Resume.CopyToAsync(stream);
                    }

                    resumePath = fileName;
                }


                if (user.UserId > 0)
                {
                    string updateSql = @"
                    UPDATE Users
                    SET UserName = @UserName,
                        FirstName = @FirstName,
                        LastName = @LastName,
                        Email = @Email,
                        PasswordHash = @PasswordHash,
                        Role = @Role,
                        Status = @Status
                    WHERE UserId = @UserId";
                    var rowsAffected = await _sqlQueryHelper.ExecuteAsync(updateSql, user);
                    if (rowsAffected > 0)
                    {
                        return CreatedAtAction(nameof(Get), new { userId = user.UserId }, new { UserId = user.UserId });
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                user.CreatedAt = DateTime.UtcNow;
                user.ProfileImagePath = profilePath;
                // Insert User
                string userSql = @"
INSERT INTO Users (UserName, PasswordHash, Role, Status, ProfileImagePath, Email, MobileNo, StreetAddress, City, District, State, Country, PinCode, IsDeleted, CreatedAt, UpdatedAt)
VALUES (@UserName, @PasswordHash, @Role, @Status, @ProfileImagePath, @Email, @MobileNo, @StreetAddress, @City, @District, @State, @Country, @PinCode, @IsDeleted, @CreatedAt, @UpdatedAt);
SELECT CAST(SCOPE_IDENTITY() as bigint);
";

                long? newUserId = await _sqlQueryHelper.GetSingleAsync<long>(userSql, user);
                if (newUserId == null)
                {
                    return BadRequest("Failed to create user.");
                }
                user.UserId = newUserId.Value;
                if (user.Role == UserRole.Student.ToString() && user.Student != null)
                {
                    user.Student.UserId = user.UserId;
                    user.Student.ResumeFilePath = resumePath;

                    string studentSql = @"
Insert into Student (UserId, FirstName, MiddleName, LastName, DateOfBirth, Nationality, Gender, BloodGroup, EnrollmentNo, Department, PassingYear, CGPA, ResumeFilePath, Skills, CreatedAt)
values (@UserId, @FirstName, @MiddleName, @LastName, @DateOfBirth, @Nationality, @Gender, @BloodGroup, @EnrollmentNo, @Department, @PassingYear, @CGPA, @ResumeFilePath, @Skills, @CreatedAt)
";
                    _ = await _sqlQueryHelper.ExecuteAsync(studentSql, user.Student);


                    return CreatedAtAction(nameof(Get), new { userId = newUserId });

                }
                else if (user.Role == UserRole.Company.ToString() && user.Company != null)
                {
                    user.Company.UserId = user.UserId;
                    string companySql = @"
Insert into Company (UserId, CompanyName, Industry, CompanySize, Website, Description, CreatedAt)
values (@varUserId, @CompanyName, @Industry, @CompanySize, @Website, @Description, @CreatedAt)
";
                    _ = await _sqlQueryHelper.ExecuteAsync(companySql, user.Company);
                    return CreatedAtAction(nameof(Get), new { userId = newUserId });
                }
                return Ok(user);
            } catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
