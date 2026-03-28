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

        [HttpGet("profile/{userId}")]
        public async Task<ActionResult<ResponseModel>> GetProfile(long userId)
        {
            try
            {
                UserModel? user = await GetUserWithDetails(userId);
                if (user == null)
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "User not found."
                    });
                }

                return Ok(new ResponseModel
                {
                    Status = ResponseStatus.Success,
                    Message = "Profile fetched successfully.",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                return Ok(new ResponseModel
                {
                    Status = ResponseStatus.Failure,
                    Message = $"Error - {ex.Message}",
                    Data = ex
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] UserCreateDTO reqestData)
        {
            try
            {
                if (reqestData == null)
                {
                    return BadRequest("User data is required.");
                }

                if (string.IsNullOrEmpty(reqestData.Data))
                {
                    return BadRequest("User data is required.");
                }

                UserModel? user = System.Text.Json.JsonSerializer.Deserialize<UserModel>(reqestData.Data, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                });
                if (user == null)
                {
                    return BadRequest("Invalid user data.");
                }

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
                string sql = "select * from Users with(nolock) where Email = @Email";
                List<UserModel> userList = await _sqlQueryHelper.GetListAsync<UserModel>(sql, new { user.Email });

                if (userList.Any())
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "Email Id Already Registered!",
                        Data = userList
                    });
                }

                string profilePath = "";
                string resumePath = "";

                if (reqestData.ProfilePhoto != null)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(reqestData.ProfilePhoto.FileName);

                    string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "ProfilePhotos");

                    if (!Directory.Exists(uploadFolder))
                    {
                        _ = Directory.CreateDirectory(uploadFolder);
                    }

                    string path = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await reqestData.ProfilePhoto.CopyToAsync(stream);
                    }

                    profilePath = path;
                }

                if (reqestData.Resume != null)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(reqestData.Resume.FileName);

                    string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Resumes");

                    if (!Directory.Exists(uploadFolder))
                    {
                        _ = Directory.CreateDirectory(uploadFolder);
                    }

                    string path = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await reqestData.Resume.CopyToAsync(stream);
                    }

                    resumePath = path;
                }


                if (user.UserId > 0)
                {
                    return NotFound();
                }
                user.CreatedAt = DateTime.UtcNow;
                user.ProfileImagePath = profilePath;
                user.PasswordHash = "Pass@123";
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
Insert into Students (UserId, FirstName, MiddleName, LastName, DateOfBirth, Nationality, Gender, BloodGroup, EnrollmentNo, Department, PassingYear, CGPA, ResumeFilePath, Skills, CreatedAt)
values (@UserId, @FirstName, @MiddleName, @LastName, @DateOfBirth, @Nationality, @Gender, @BloodGroup, @EnrollmentNo, @Department, @PassingYear, @CGPA, @ResumeFilePath, @Skills, @CreatedAt)
";
                    _ = await _sqlQueryHelper.ExecuteAsync(studentSql, user.Student);


                    return CreatedAtAction(nameof(Get), new { userId = newUserId });

                }
                else if (user.Role == UserRole.Company.ToString() && user.Company != null)
                {
                    user.Company.UserId = user.UserId;
                    string companySql = @"
Insert into Companies (UserId, CompanyName, Industry, CompanySize, Website, Description, CreatedAt)
values (@varUserId, @CompanyName, @Industry, @CompanySize, @Website, @Description, @CreatedAt)
";
                    _ = await _sqlQueryHelper.ExecuteAsync(companySql, user.Company);
                    return CreatedAtAction(nameof(Get), new { userId = newUserId });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("profile")]
        public async Task<ActionResult<ResponseModel>> UpdateProfile([FromBody] UserModel request)
        {
            try
            {
                if (request == null || request.UserId <= 0)
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "Invalid user data."
                    });
                }

                UserModel? existingUser = await GetUserWithDetails(request.UserId);
                if (existingUser == null || existingUser.IsDeleted)
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "User not found."
                    });
                }

                request.UserName = existingUser.UserName;
                request.Email = existingUser.Email;
                request.PasswordHash = existingUser.PasswordHash;
                request.Role = existingUser.Role;
                request.Status = existingUser.Status;
                request.ProfileImagePath = existingUser.ProfileImagePath;
                request.CreatedAt = existingUser.CreatedAt;
                request.IsDeleted = existingUser.IsDeleted;
                request.UpdatedAt = DateTime.UtcNow;

                string userSql = @"
UPDATE Users
SET MobileNo = @MobileNo,
    StreetAddress = @StreetAddress,
    City = @City,
    District = @District,
    State = @State,
    Country = @Country,
    PinCode = @PinCode,
    UpdatedAt = @UpdatedAt
WHERE UserId = @UserId";
                _ = await _sqlQueryHelper.ExecuteAsync(userSql, request);

                if (request.Role == UserRole.Student && request.Student != null)
                {
                    request.Student.UserId = request.UserId;
                    request.Student.ResumeFilePath = existingUser.Student?.ResumeFilePath;
                    request.Student.CreatedAt = existingUser.Student?.CreatedAt ?? request.Student.CreatedAt;

                    string studentSql = @"
UPDATE Students
SET FirstName = @FirstName,
    MiddleName = @MiddleName,
    LastName = @LastName,
    DateOfBirth = @DateOfBirth,
    Nationality = @Nationality,
    Gender = @Gender,
    BloodGroup = @BloodGroup,
    EnrollmentNo = @EnrollmentNo,
    Department = @Department,
    PassingYear = @PassingYear,
    CGPA = @CGPA,
    Skills = @Skills
WHERE UserId = @UserId";
                    _ = await _sqlQueryHelper.ExecuteAsync(studentSql, request.Student);
                }
                else if (request.Role == UserRole.Company && request.Company != null)
                {
                    request.Company.UserId = request.UserId;
                    request.Company.CreatedAt = existingUser.Company?.CreatedAt ?? request.Company.CreatedAt;
                    request.Company.LogoUrl = existingUser.Company?.LogoUrl;
                    request.Company.ContactEmail = existingUser.Email;

                    string companySql = @"
UPDATE Companies
SET CompanyName = @CompanyName,
    Website = @Website,
    Description = @Description,
    Industry = @Industry,
    Location = @Location,
    HRName = @HRName,
    ContactEmail = @ContactEmail,
    ContactPhone = @ContactPhone,
    FoundedYear = @FoundedYear,
    CompanySize = @CompanySize
WHERE UserId = @UserId";
                    _ = await _sqlQueryHelper.ExecuteAsync(companySql, request.Company);
                }

                UserModel? updatedUser = await GetUserWithDetails(request.UserId);

                return Ok(new ResponseModel
                {
                    Status = ResponseStatus.Success,
                    Message = "Profile updated successfully.",
                    Data = updatedUser
                });
            }
            catch (Exception ex)
            {
                return Ok(new ResponseModel
                {
                    Status = ResponseStatus.Failure,
                    Message = $"Error - {ex.Message}",
                    Data = ex
                });
            }
        }

        [HttpPut("change-password")]
        public async Task<ActionResult<ResponseModel>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (request == null || request.UserId <= 0)
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "Invalid password request."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.OldPassword) ||
                    string.IsNullOrWhiteSpace(request.NewPassword) ||
                    string.IsNullOrWhiteSpace(request.VerifyNewPassword))
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "All password fields are required."
                    });
                }

                if (request.NewPassword != request.VerifyNewPassword)
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "New password and verify password must match."
                    });
                }

                UserModel? user = await _sqlQueryHelper.GetSingleAsync<UserModel>(
                    "SELECT TOP 1 * FROM Users WITH(NOLOCK) WHERE UserId = @UserId",
                    new { request.UserId });

                if (user == null || user.IsDeleted)
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "User not found."
                    });
                }

                if (user.PasswordHash != request.OldPassword)
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "Old password is incorrect."
                    });
                }

                if (request.OldPassword == request.NewPassword)
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "New password must be different from old password."
                    });
                }

                _ = await _sqlQueryHelper.ExecuteAsync(
                    @"UPDATE Users SET PasswordHash = @NewPassword, UpdatedAt = @UpdatedAt WHERE UserId = @UserId",
                    new
                    {
                        request.UserId,
                        request.NewPassword,
                        UpdatedAt = DateTime.UtcNow
                    });

                return Ok(new ResponseModel
                {
                    Status = ResponseStatus.Success,
                    Message = "Password updated successfully."
                });
            }
            catch (Exception ex)
            {
                return Ok(new ResponseModel
                {
                    Status = ResponseStatus.Failure,
                    Message = $"Error - {ex.Message}",
                    Data = ex
                });
            }
        }

        private async Task<UserModel?> GetUserWithDetails(long userId)
        {
            string sql = "SELECT TOP 1 * FROM Users WITH(NOLOCK) WHERE UserId = @UserId";
            UserModel? user = await _sqlQueryHelper.GetSingleAsync<UserModel>(sql, new { UserId = userId });
            if (user == null)
            {
                return null;
            }

            if (user.Role == UserRole.Student)
            {
                user.Student = await _sqlQueryHelper.GetSingleAsync<StudentModel>(
                    "SELECT TOP 1 * FROM Students WITH(NOLOCK) WHERE UserId = @UserId",
                    new { UserId = userId });
            }
            else if (user.Role == UserRole.Company)
            {
                user.Company = await _sqlQueryHelper.GetSingleAsync<CompanyModel>(
                    "SELECT TOP 1 * FROM Companies WITH(NOLOCK) WHERE UserId = @UserId",
                    new { UserId = userId });
            }

            return user;
        }
    }
}
