using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Slices.Sale;

[ApiController]
[Route("api/sales")]
public class CreateProductSaleController(CreateProductSaleCommandHandler commandHandler) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request, CancellationToken ct)
    {
        var command = new CreateProductSaleCommand
        {
            Ean = request.Ean,
            ClientName = request.ClientName,
            SalePrice = request.SalePrice,
            Quantity = request.Quantity,
        };

        await commandHandler.HandleAsync(command, ct);
        return Created();
    }
}

public record CreateSaleRequest(string Ean, string ClientName, double SalePrice, int Quantity);
