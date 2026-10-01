using Microsoft.AspNetCore.Mvc;
using OrderSystem.Application;
using OrderSystem.Application.Products;

namespace OrderSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _svc;
    public ProductsController(IProductService svc) => _svc = svc;

    // Listeleme ?q=...&onlyActive=true&page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? q, [FromQuery] bool? onlyActive,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 100) pageSize = 100;

        var result = await _svc.ListAsync(q, onlyActive, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var vm = await _svc.GetAsync(id, ct);
        return vm is null ? NotFound() : Ok(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductCreateDto dto, CancellationToken ct)
    {
        var (ok, err, id) = await _svc.CreateAsync(dto, ct);
        if (!ok) return BadRequest(new { message = err });
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto, CancellationToken ct)
    {
        var (ok, err) = await _svc.UpdateAsync(id, dto, ct);
        return ok ? NoContent() : BadRequest(new { message = err });
    }

    // Stok artı/eksi (PATCH /api/products/5/stock)
    [HttpPatch("{id:int}/stock")]
    public async Task<IActionResult> AdjustStock(int id, [FromBody] ProductAdjustStockDto dto, CancellationToken ct)
    {
        var (ok, err) = await _svc.AdjustStockAsync(id, dto, ct);
        return ok ? NoContent() : BadRequest(new { message = err });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await _svc.SoftDeleteAsync(id, ct) ? NoContent() : NotFound();
}