using API.Model;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected ActionResult<ResponseModel> Success(string message, object? data = null)
        {
            return Ok(new ResponseModel
            {
                Status = ResponseStatus.Success,
                Message = message,
                Data = data
            });
        }

        protected ActionResult<ResponseModel> Failure(string message, object? data = null)
        {
            return Ok(new ResponseModel
            {
                Status = ResponseStatus.Failure,
                Message = message,
                Data = data
            });
        }
    }
}
