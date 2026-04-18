using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Slices.ProductInventory;

[ApiController]
[Route("api/inventory")]
public class ProductInventoryController(ProductInventoryQueryHandler queryHandler) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var inventory = await queryHandler.HandleAsync(new GetAllProductInventoryQuery(), ct);
        return Ok(inventory);
    }
}
