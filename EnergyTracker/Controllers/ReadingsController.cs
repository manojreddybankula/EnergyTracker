using EnergyTracker.Domains;
using EnergyTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EnergyTracker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReadingsController : ControllerBase
    {
        private readonly EnergyService _service;
        private readonly ILogger<ReadingsController> _logger;

        public ReadingsController(EnergyService service, ILogger<ReadingsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> UploadReadings([FromBody] UploadReadingsRequest request)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while uploading readings");
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }
    }
}
