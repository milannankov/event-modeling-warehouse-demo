using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Slices.NewProduct;

[ApiController]
[Route("api/products")]
public class CreateProductController(CreateProductCommandHandler commandHandler) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var command = new CreateProductCommand
        {
            Name = request.Name,
            Ean = request.Ean,
        };

        await commandHandler.HandleAsync(command, ct);
        return Created();
    }
}

public record CreateProductRequest(string Name, string Ean);
