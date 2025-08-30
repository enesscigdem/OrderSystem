using Microsoft.AspNetCore.Mvc;
using OrderSystem.Application;
using OrderSystem.Application.Abstraction;
using OrderSystem.Application.Orders;

namespace OrderSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;
    public OrdersController(IOrderService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto, CancellationToken ct)
    {
        var (ok, err, id) = await _service.CreateAsync(dto, ct);
        if (!ok) return BadRequest(new { message = err });
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string userId, CancellationToken ct)
        => Ok(await _service.ListByUserAsync(userId, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
    {
        var data = await _service.GetAsync(id, ct);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        => await _service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}