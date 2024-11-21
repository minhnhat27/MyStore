using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Services.Statistics;

namespace MyStore.Presentation.Controllers
{
    [Route("api/statistics")]
    [ApiController]
    [Authorize(Roles = "Admin,Employee")]
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

        //[HttpGet("revenue-this-year")]
        //public async Task<IActionResult> GetRevenueThisYear()
        //{
        //    try
        //    {
        //        var results = await _statisticsService.GetRevenueInYear();
        //        return Ok(results);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //}

        [HttpGet("revenue/{year}")]
        public async Task<IActionResult> GetRevenueInYear(int year)
        {
            try
            {
                var results = await _statisticsService.GetRevenueInYear(year);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("revenue/{year}/{month}")]
        public async Task<IActionResult> GetRevenueInMonth(int year, int month)
        {
            try
            {
                var results = await _statisticsService.GetRevenue(month, year);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
