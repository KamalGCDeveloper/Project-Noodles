using Microsoft.AspNetCore.Mvc;
using Noodle.Api.Services;
using Noodle.Api.Models;

namespace Noodle.Api.Controllers
{
    [ApiController]
    [Route("noodle")]
    public class StablecoinsController : ControllerBase
    {
        private readonly IStablecoinsService _service;

        public StablecoinsController(IStablecoinsService service)
        {
            _service = service;
        }

        [HttpGet("top-growth-stablecoins")]
        public async Task<ActionResult<TopGrowthStablecoin>> GetTopGrowthStablecoins()
        {
            var result = await _service.GetTopGrowthStablecoinsAsync();
            return Ok(result);
        }

        [HttpGet("most-talked-about-stablecoins")]
        public async Task<ActionResult<List<MostTalkedStablecoin>>> GetMostTalkedAboutStablecoins()
        {
            var data = await _service.GetMostTalkedStablecoinsAsync();
            return Ok(data);
        }

        [HttpGet("stablecoins-number-tracked")]
        public async Task<ActionResult<TrackedStablecoin>> GetStablecoinsNumberTracked()
        {
            var data = await _service.GetNumberTrackedStablecoinsAsync();
            return Ok(data);
        }

        [HttpGet("active-users-stablecoins")]
        public async Task<ActionResult<ActiveUsersStablecoin>> GetActiveUsersStablecoins()
        {
            var data = await _service.GetActiveUsersStablecoinsAsync();
            return Ok(data);
        }

        [HttpGet("stablecoins")]
        public async Task<ActionResult<StablecoinListResponse>> GetStablecoins([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int limit = 20, [FromQuery] string? sortBy = null, [FromQuery] string? sortDir = null)
        {
            var data = await _service.GetStablecoinsAsync(q, page, limit, sortBy, sortDir);
            return Ok(data);
        }
    }
}