using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Slices.AvailableProducts;

[ApiController]
[Route("api/sales/products")]
public class AvailableProductsController(AvailableProductsQueryHandler queryHandler) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var products = await queryHandler.HandleAsync(new GetAllAvailableProductsQuery(), ct);
        return Ok(products);
    }
}
