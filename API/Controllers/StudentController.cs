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
            string sql = "SELECT * FROM Students With(nolock)";
            List<StudentModel>? studentList = await _sqlQueryHelper.GetListAsync<StudentModel>(sql);
            for(int i = 0; i < studentList.Count; i++)
            {
                var student = studentList[i];
                string sql2 = "SELECT * FROM Users with(nolock) WHERE UserId = @UserId";
                UserModel? user = await _sqlQueryHelper.GetSingleAsync<UserModel>(sql2, new { student.UserId });
                studentList[i].User = user;
            };
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
