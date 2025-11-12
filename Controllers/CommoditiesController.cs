using Microsoft.AspNetCore.Mvc;
using Noodle.Api.Services;
using System.Threading.Tasks;

namespace Noodle.Api.Controllers
{
    [ApiController]
    [Route("noodle")]
    public class CommoditiesController : ControllerBase
    {
        private readonly ICommoditiesService _service;

        public CommoditiesController(ICommoditiesService service)
        {
            _service = service;
        }

        [HttpGet("top-growth-commodities")]
        public async Task<IActionResult> GetTopGrowthCommodities()
        {
            var result = await _service.GetTopGrowthCommoditiesAsync();
            return Ok(result);
        }
        [HttpGet("most-talked-about-commodities")]
        public async Task<IActionResult> GetMostTalkedAboutCommodities()
        {
            var result = await _service.GetMostTalkedAboutCommoditiesAsync();
            return Ok(result);
        }
        [HttpGet("commodities-number-tracked")]
        public async Task<IActionResult> GetNumberTrackedCommodities()
        {
            var result = await _service.GetNumberTrackedCommoditiesAsync();
            return Ok(result);
        }
        [HttpGet("active-users-commodities")]
        public async Task<IActionResult> GetTotalActiveUsers7dCommodity()
        {
            var result = await _service.GetTotalActiveUsers7dCommodityAsync();
            return Ok(result);
        }
        [HttpGet("commodities")]
        public async Task<IActionResult> GetCommoditiesHealthRanks([FromQuery] int limit = 10, [FromQuery] int page = 1, [FromQuery] string? groupFilter = null)
        {
            var result = await _service.GetCommoditiesHealthRanksAsync(limit, page, groupFilter);
            return Ok(result);
        }
    }
}