using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared;

namespace WebApp.Controllers
{
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
