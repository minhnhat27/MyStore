using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Services;

namespace MyStore.Presentation.Controllers
{
    [Route("api/statistics")]
    [ApiController]
    public class StatisticsController(IStatisticsService statisticsService) : ControllerBase
    {
        private readonly IStatisticsService _statisticsService = statisticsService;

        [HttpGet("revenue-this-year")]
        public async Task<IActionResult> GetRevenueThisYear()
        {
            try
            {
                var results = await _statisticsService.GetRevenueInYear();
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
