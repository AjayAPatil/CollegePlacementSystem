using API.Common;
using API.Constants;
using Microsoft.AspNetCore.Http;
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
            _sqlQueryHelper.Execute(InitialQuery.DropAndCreateTables); // Test database connection
            return Ok("Auth endpoint is working!");
        }
    }
}
