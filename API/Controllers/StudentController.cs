using API.Common;
using API.Model;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {

        private readonly ISqlQueryHelper _sqlQueryHelper;

        public StudentController(ISqlQueryHelper sqlQueryHelper)
        {
            _sqlQueryHelper = sqlQueryHelper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentModel>>> Get()
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

            return Ok(studentList ?? []);
        }

        [HttpGet("{studentId}")]
        public async Task<ActionResult<StudentModel>> Get(long studentId)
        {
            string sql = "SELECT * FROM Students With(nolock) where Id = @id";
            StudentModel? student = await _sqlQueryHelper.GetSingleAsync<StudentModel>(sql, new { id = studentId });
            return Ok(student ?? new StudentModel());
        }
    }
}
