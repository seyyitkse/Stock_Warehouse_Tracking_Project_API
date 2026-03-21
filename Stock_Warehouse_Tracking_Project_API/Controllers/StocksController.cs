using Microsoft.AspNetCore.Mvc;
using Stock_Warehouse_Tracking_Project_API.Models.Stock;
using Stock_Warehouse_Tracking_Project_API.Services;

namespace Stock_Warehouse_Tracking_Project_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StocksController : ControllerBase
    {
        private readonly IStockService _stockService;

        public StocksController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<StockDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var stocks = await _stockService.GetStocksAsync(cancellationToken);
            return Ok(stocks);
        }
    }
}
