using API.Common;
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
            string sql = "select * from Companies with(nolock)";
            List<CompanyModel> companyList = await _sqlQueryHelper.GetListAsync<CompanyModel>(sql);
            return Ok(companyList);
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel>> Post([FromForm] CompanyCreateDTO requestData)
        {
            try
            {
                if (requestData == null)
                {
                    return BadRequest("User data is required.");
                }

                if (string.IsNullOrEmpty(requestData.Data))
                {
                    return BadRequest("User data is required.");
                }

                CompanyModel? company = System.Text.Json.JsonSerializer.Deserialize<CompanyModel>(requestData.Data, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                });
                if (company == null)
                {
                    return BadRequest("Invalid company data.");
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

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await requestData.CompanyLogo.CopyToAsync(stream);
                    }

                    company.LogoUrl = path;
                }

                string sql = "select * from Companies with(nolock) where ContactEmail = @ContactEmail";
                List<CompanyModel> companyList = await _sqlQueryHelper.GetListAsync<CompanyModel>(sql, new { company.ContactEmail });

                if(companyList.Any())
                {
                    return Ok(new ResponseModel
                    {
                        Status = ResponseStatus.Failure,
                        Message = "Company already registered!",
                        Data = companyList
                    });
                };


                return Ok(companyList);
            }
            catch (Exception ex)
            {
                return Ok();
            }
        }
    }
}
