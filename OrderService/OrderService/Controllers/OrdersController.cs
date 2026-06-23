using Microsoft.AspNetCore.Mvc;
using OrderService.Api.Extensions;
using OrderService.Application.Abstractions;
using OrderService.Application.DTOs.Orders;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
    [FromBody] CreateOrderDto createOrderDto,
    CancellationToken ct)
    {
        var result = await orderService.CreateAsync(createOrderDto, ct);
        return result.Match(id => CreatedAtAction(nameof(GetByIdAsync), new { id }, new { id }));
    }

    [HttpGet("{id}")]
    [ActionName(nameof(GetByIdAsync))]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var result = await orderService.GetByIdAsync(id, ct);
        return result.Match(Ok);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(int page, int pageSize, DateTime from, DateTime to, CancellationToken ct)
    {
        var result = await orderService.GetAllAsync(page, pageSize, from, to, ct);
        return result.Match(Ok);
    }

    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> ConfirmAsync(Guid id, CancellationToken ct)
    {
        var result = await orderService.ConfirmAsync(id, ct);
        return result.Match(_ => NoContent());
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelAsync(Guid id, CancellationToken ct)
    {
        var result = await orderService.CancelAsync(id, ct);
        return result.Match(_ => NoContent());
    }
}
