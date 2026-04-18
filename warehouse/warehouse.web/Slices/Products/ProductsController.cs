using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Slices.Products;

[ApiController]
[Route("api/products")]
public class ProductsController(ProductsQueryHandler queryHandler) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var products = await queryHandler.HandleAsync(new GetAllProductsQuery(), ct);
        return Ok(products);
    }
}
