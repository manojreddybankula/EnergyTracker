using Microsoft.AspNetCore.Mvc;
using EnergyTracker.Services;

namespace EnergyTracker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly EnergyService _service;

        public ReportController(EnergyService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetReport([FromQuery] string userId, [FromQuery] string groupBy, [FromQuery] string product)
        {
            try
            {
                var report = await _service.GetAggregatedReportAsync(userId, groupBy, product);
                return Ok(report);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
