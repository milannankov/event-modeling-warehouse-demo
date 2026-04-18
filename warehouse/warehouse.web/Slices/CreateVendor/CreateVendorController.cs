using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Slices.CreateVendor;

[ApiController]
[Route("api/vendors")]
public class CreateVendorController(CreateVendorCommandHandler commandHandler) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVendorRequest request, CancellationToken ct)
    {
        var command = new CreateVendorCommand
        {
            EuVat = request.EuVat,
            Name = request.Name,
        };

        await commandHandler.HandleAsync(command, ct);
        return Created();
    }
}

public record CreateVendorRequest(string EuVat, string Name);
