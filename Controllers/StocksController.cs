using Microsoft.AspNetCore.Mvc;
using Noodle.Api.Services;
using Noodle.Api.Models;
using Noodle.Api.Repositories;

namespace Noodle.Api.Controllers
{
    [ApiController]
    [Route("noodle")]
    public class StocksController : ControllerBase
    {
        private readonly IStocksService _service;

        public StocksController(IStocksService service)
        {
            _service = service;
        }

        [HttpGet("top-growth-stocks")]
        public async Task<ActionResult<TopGrowthStock>> GetTopGrowthStocks()
        {
            var data = await _service.GetTopGrowthStocksAsync();
            return Ok(data);
        }

        [HttpGet("most-talked-about-stocks")]
        public async Task<ActionResult<MostTalkedStock>> GetMostTalkedAboutStocks()
        {
            var data = await _service.GetMostTalkedAboutStocksAsync();
            return Ok(data);
        }

        [HttpGet("stock-number-tracked")]
        public async Task<ActionResult<TrackedStock>> GetNumberTracked()
        {
            var data = await _service.GetNumberTrackedAsync();
            return Ok(data);
        }

        [HttpGet("active-users-stock")]
        public async Task<ActionResult<ActiveUsersStock>> GetTotalActiveUsers7d()
        {
            var data = await _service.GetTotalActiveUsers7dAsync();
            return Ok(data);
        }

        [HttpGet("stocks")]
        public async Task<IActionResult> GetStockHealthRanks(int limit = 25, int page = 1, string? search = null, string? groupFilter = null)
        {
            var result = await _service.GetStockHealthRanksAsync(limit, page, search, groupFilter);
            return Ok(result);
        }
    }
}