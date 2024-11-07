using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Services.Statistics;

namespace MyStore.Presentation.Controllers
{
    [Route("api/statistics")]
    [ApiController]
    public class StatisticsController(IStatisticsService statisticsService) : ControllerBase
    {
        private readonly IStatisticsService _statisticsService = statisticsService;

        [HttpGet("general")]
        public async Task<IActionResult> GetGeneral()
        {
            try
            {
                var results = await _statisticsService.GeneralStatistics();
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

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
