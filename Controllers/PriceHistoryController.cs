using Microsoft.AspNetCore.Mvc;
using Noodle.Api.Services;
using Noodle.Api.Models;

namespace Noodle.Api.Controllers
{
    [ApiController]
    [Route("noodle")]
    public class PriceHistoryController : ControllerBase
    {
        private readonly IPriceHistoryService _service;

        public PriceHistoryController(IPriceHistoryService service)
        {
            _service = service;
        }

        [HttpGet("price-history")]
        public async Task<IActionResult> Get([FromQuery] PriceHistoryRequest req)
        {
            var data = await _service.GetPriceHistory(req);
            return Ok(data);
        }
    }
}