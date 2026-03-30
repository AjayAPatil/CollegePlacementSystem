using API.Common;
using API.Constants;
using API.Model;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : BaseApiController
    {
        private readonly ISqlQueryHelper _sqlQueryHelper;

        public AuthController(ISqlQueryHelper sqlQueryHelper)
        {
            _sqlQueryHelper = sqlQueryHelper;
        }

        [HttpGet]
        public ActionResult<ResponseModel> DropAndCreateTables()
        {
            try
            {
                _ = _sqlQueryHelper.Execute(InitialQuery.DropAndCreateTables);
                return Success("Auth endpoint is working.");
            }
            catch (Exception ex)
            {
                return Failure($"Error - {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel>> Login(UserModel? inputUser)
        {
            try
            {
                if (inputUser == null)
                {
                    return Failure("Invalid Input");
                }
                if (string.IsNullOrEmpty(inputUser.Email) || string.IsNullOrEmpty(inputUser.PasswordHash))
                {
                    return Failure("UserName and Password are required!");
                }

                string sql = "select top 1 * from Users with(nolock) where email = @Email";
                UserModel? user = await _sqlQueryHelper.GetSingleAsync<UserModel>(sql, new { inputUser.Email });
                if (user == null)
                {
                    return Failure("User not found!");
                }
                if (user.PasswordHash != inputUser.PasswordHash)
                {
                    return Failure("Invalid Password!");
                }
                if (user.Status != UserStatus.Active)
                {
                    return Failure("User is not active");
                }
                if (user.IsDeleted)
                {
                    return Failure("User is deleted");
                }

                if (user.Role == UserRole.Student)
                {
                    sql = "select top 1 * from Students with(nolock) where UserId = @UserId";
                    StudentModel? student = await _sqlQueryHelper.GetSingleAsync<StudentModel>(sql, new { user.UserId });
                    if (student == null)
                    {
                        return Failure("Student Not Found!");
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
                        return Failure("Company Not Found!");
                    }
                    user.Company = company;
                }
                else
                {
                    return Failure("Role Not Assigned!");
                }

                return Success("Logged In Successfully!", user);
            }
            catch (Exception ex)
            {
                return Failure($"Error - {ex.Message}");
            }
        }
    }
}
