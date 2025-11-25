using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Noodle.Api.Models;
using Noodle.Api.Services;

namespace Noodle.Api.Controllers
{
    [ApiController]
    [Route("noodle")]
    public class ComparisonController : ControllerBase
    {
        private readonly IComparisonService _service;

        public ComparisonController(IComparisonService service)
        {
            _service = service;
        }

        /// <summary>
        /// Compare 2–4 assets (stablecoins / stocks / commodities) side-by-side.
        /// </summary>
        [HttpPost("compare")]
        public async Task<IActionResult> Compare([FromBody] ComparisonRequest req)
        {
            if (req.AssetIds == null || req.AssetIds.Count < 2)
                return BadRequest(new { error = "You must provide at least 2 asset IDs" });

            if (req.AssetIds.Count > 4)
                return BadRequest(new { error = "Maximum 4 assets allowed" });

            try
            {
                var result = await _service.CompareAsync(req);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}