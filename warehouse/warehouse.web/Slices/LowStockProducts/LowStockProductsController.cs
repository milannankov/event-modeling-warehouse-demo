using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Slices.LowStockProducts;

[ApiController]
[Route("api/low-stock")]
public class LowStockProductsController(LowStockProductsQueryHandler queryHandler) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await queryHandler.HandleAsync(new GetAllLowStockProductsQuery(), ct);
        return Ok(items);
    }
}
