using API.Common;
using API.Constants;
using API.Model;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISqlQueryHelper _sqlQueryHelper;

        public AuthController(ISqlQueryHelper sqlQueryHelper)
        {
            _sqlQueryHelper = sqlQueryHelper;
        }

        [HttpGet]
        public ActionResult<string> DropAndCreateTables()
        {
            _ = _sqlQueryHelper.Execute(InitialQuery.DropAndCreateTables); // Test database connection
            return Ok("Auth endpoint is working!");
        }

        [HttpPost]
        public async Task<ActionResult> Login(UserModel? inputUser)
        {
            try
            {
                if (inputUser == null)
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "Invalid Input",
                    });
                }
                if (string.IsNullOrEmpty(inputUser.Email) || string.IsNullOrEmpty(inputUser.PasswordHash))
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "UserName and Password are required!",
                    });
                }

                string sql = "select top 1 * from Users with(nolock) where email = @Email";
                UserModel? user = await _sqlQueryHelper.GetSingleAsync<UserModel>(sql, new { inputUser.Email });
                if (user == null)
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "User not found!",
                    });
                }
                if (user.PasswordHash != inputUser.PasswordHash)
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "Invalid Password!",
                    });
                }
                if (user.Status != UserStatus.Active)
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "User is not active",
                    });
                }
                if (user.IsDeleted)
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "User is deleted",
                    });
                }

                if (user.Role == UserRole.Student)
                {
                    sql = "select top 1 * from Students with(nolock) where UserId = @UserId";
                    StudentModel? student = await _sqlQueryHelper.GetSingleAsync<StudentModel>(sql, new { user.UserId });
                    if (student == null)
                    {
                        return Ok(new ResponseModel
                        {
                            Status = ResponseStatus.Failure,
                            Message = "Student Not Found!",
                        });
                    }
                    user.Student = student;
                }
                else if (user.Role == UserRole.Admin)
                {

                }
                else if (user.Role == UserRole.Company)
                {
                    sql = "select top 1 * from Companies with(nolock) where UserId = @UserId";
                    CompanyModel? company = await _sqlQueryHelper.GetSingleAsync<CompanyModel>(sql, new { user.UserId });
                    if (company == null)
                    {
                        return Ok(new ResponseModel
                        {
                            Status = ResponseStatus.Failure,
                            Message = "Company Not Found!",
                        });
                    }
                    user.Company = company;
                }
                else
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "Role Not Assigned!",
                    });
                }

                return Ok(new ResponseModel
                {
                    Status = ResponseStatus.Success,
                    Message = $"Logged In Successfully!",
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
    }
}
