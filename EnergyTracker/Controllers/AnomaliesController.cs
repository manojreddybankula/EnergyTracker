using Microsoft.AspNetCore.Mvc;
using EnergyTracker.Application.Services;

namespace EnergyTracker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AnomaliesController : ControllerBase
    {
        private readonly EnergyService _service;

        public AnomaliesController(EnergyService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAnomalies([FromQuery] string userId, [FromQuery] string period)
        {
            try
            {
                var anomalies = await _service.GetAnomaliesAsync(userId, period);
                return Ok(anomalies);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
