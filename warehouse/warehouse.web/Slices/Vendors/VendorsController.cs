using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Slices.Vendors;

[ApiController]
[Route("api/vendors")]
public class VendorsController(VendorsQueryHandler queryHandler) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var vendors = await queryHandler.HandleAsync(new GetAllVendorsQuery(), ct);
        return Ok(vendors);
    }
}
