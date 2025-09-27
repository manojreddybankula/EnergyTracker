using Microsoft.AspNetCore.Mvc;
using EnergyTracker.Application.DTOs;
using EnergyTracker.Application.Services;

namespace EnergyTracker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReadingsController : ControllerBase
    {
        private readonly EnergyService _service;

        public ReadingsController(EnergyService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> UploadReadings([FromBody] UploadReadingsRequest request)
        {
            var (success, errors) = await _service.UploadReadingsAsync(request);
            if (!success)
            {
                if (errors.Any(e => e.Contains("Duplicate")))
                    return Conflict(new { errors });
                return BadRequest(new { errors });
            }
            return Ok();
        }
    }
}
