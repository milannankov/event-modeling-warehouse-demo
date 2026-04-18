using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Slices.CreatePurchase;

[ApiController]
[Route("api/purchases")]
public class CreatePurchaseController(CreatePurchaseCommandHandler commandHandler) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseRequest request, CancellationToken ct)
    {
        var command = new CreatePurchaseCommand
        {
            Ean = request.Ean,
            EuVat = request.EuVat,
            Price = request.Price,
            Quantity = request.Quantity,
        };

        await commandHandler.HandleAsync(command, ct);
        return Created();
    }
}

public record CreatePurchaseRequest(string Ean, string EuVat, double Price, int Quantity);
