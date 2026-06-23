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
    public async Task<IActionResult> CreateAsync(CreateOrderDto createOrderDto, CancellationToken ct)
    {
        var result = await orderService.CreateAsync(createOrderDto, ct);
        return result.Match(id => CreatedAtAction(nameof(GetByIdAsync), id));
    }

    [HttpGet("{id}")]
    [ActionName(nameof(GetByIdAsync))]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        return Ok();
    }
}
