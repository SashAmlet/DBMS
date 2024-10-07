using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        [HttpPost]
        public IActionResult UpdateBasePath([FromBody] UpdatePathDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewBasePath))
            {
                return BadRequest("Invalid path");
            }

            Constants.BasePath = dto.NewBasePath;

            return Ok();
        }
    }
}
