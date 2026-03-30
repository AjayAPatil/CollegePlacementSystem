using API.Common;
using API.Model;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class StudentController : BaseApiController
    {

        private readonly ISqlQueryHelper _sqlQueryHelper;

        public StudentController(ISqlQueryHelper sqlQueryHelper)
        {
            _sqlQueryHelper = sqlQueryHelper;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel>> Get()
        {
            try
            {
                string sql = @"
SELECT
    s.*,
    selectedCompany.CompanyName AS SelectedCompanyName
FROM Students s WITH(NOLOCK)
OUTER APPLY
(
    SELECT TOP 1 c.CompanyName
    FROM JobApplications ja WITH(NOLOCK)
    INNER JOIN Companies c WITH(NOLOCK) ON c.Id = ja.CompanyId
    WHERE ja.StudentId = s.Id
      AND ja.Status = 'accepted'
    ORDER BY ja.DecisionAt DESC, ja.UpdatedAt DESC, ja.ApplicationId DESC
) selectedCompany";
                List<StudentModel>? studentList = await _sqlQueryHelper.GetListAsync<StudentModel>(sql);
                for (int i = 0; i < studentList.Count; i++)
                {
                    var student = studentList[i];
                    string sql2 = @"
SELECT
    UserId,
    UserName,
    Role,
    Status,
    ProfileImagePath,
    Email,
    MobileNo,
    StreetAddress,
    City,
    District,
    State,
    Country,
    PinCode,
    IsDeleted,
    CreatedAt,
    UpdatedAt
FROM Users WITH(NOLOCK)
WHERE UserId = @UserId";
                    UserModel? user = await _sqlQueryHelper.GetSingleAsync<UserModel>(sql2, new { student.UserId });
                    studentList[i].User = user;
                }

                return Success("Students retrieved successfully.", studentList ?? []);
            }
            catch (Exception ex)
            {
                return Failure($"Error - {ex.Message}");
            }
        }

        [HttpGet("{studentId}")]
        public async Task<ActionResult<ResponseModel>> Get(long studentId)
        {
            try
            {
                if (studentId <= 0)
                {
                    return Failure("Student id is required.");
                }

                string sql = "SELECT * FROM Students With(nolock) where Id = @id";
                StudentModel? student = await _sqlQueryHelper.GetSingleAsync<StudentModel>(sql, new { id = studentId });
                if (student == null)
                {
                    return Failure("Student not found.");
                }

                return Success("Student retrieved successfully.", student);
            }
            catch (Exception ex)
            {
                return Failure($"Error - {ex.Message}");
            }
        }
    }
}
