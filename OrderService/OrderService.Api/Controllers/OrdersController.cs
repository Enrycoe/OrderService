using Microsoft.AspNetCore.Mvc;
using OrderService.Api.Extensions;
using OrderService.Api.Models;
using OrderService.Application.Abstractions;
using OrderService.Application.DTOs.Orders;
using OrderService.Domain.Models;

namespace OrderService.Api.Controllers;

/// <summary>
/// Operações relacionadas ao gerenciamento de pedidos.
/// </summary>
[ApiController]
[Route("orders")]
[Produces("application/json")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    /// <summary>
    /// Cria um novo pedido.
    /// </summary>
    /// <param name="createOrderDto">
    /// Dados do pedido contendo cliente, moeda e itens.
    /// </param>
    /// <response code="201">Pedido criado com sucesso.</response>
    /// <response code="400">Dados inválidos ou regras de negócio violadas.</response>
    /// <response code="404">Algum produto informado não foi encontrado.</response>
    /// <response code="409">Estoque insuficiente para algum produto.</response>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateOrderDto createOrderDto,
        CancellationToken ct)
    {
        var result = await orderService.CreateAsync(createOrderDto, ct);

        return result.Match(id =>
            CreatedAtAction(nameof(GetByIdAsync), new { id }, new { id }));
    }

    /// <summary>
    /// Obtém um pedido pelo identificador.
    /// </summary>
    /// <param name="id">Identificador único do pedido.</param>
    /// <response code="200">Pedido encontrado.</response>
    /// <response code="404">Pedido não encontrado.</response>
    [HttpGet("{id:guid}")]
    [ActionName(nameof(GetByIdAsync))]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(
        Guid id,
        CancellationToken ct)
    {
        var result = await orderService.GetByIdAsync(id, ct);

        return result.Match(Ok);
    }

    /// <summary>
    /// Lista pedidos de forma paginada.
    /// </summary>
    /// <param name="page">Número da página.</param>
    /// <param name="pageSize">Quantidade de registros por página.</param>
    /// <param name="from">Data inicial para filtro de criação.</param>
    /// <param name="to">Data final para filtro de criação.</param>
    /// <response code="200">Lista de pedidos retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedList<OrderListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(
        int? page,
        int? pageSize,
        int? customerId,
        int? status,
        DateTime? from,
        DateTime? to,

        CancellationToken ct)
    {
        var result = await orderService.GetAllAsync(
            page,
            pageSize,
            customerId,
            status,
            from,
            to,
            ct);

        return result.Match(Ok);
    }

    /// <summary>
    /// Confirma um pedido.
    /// </summary>
    /// <remarks>
    /// Ao confirmar um pedido:
    /// - O status deve estar como Placed.
    /// - O estoque dos produtos será debitado.
    /// - Caso ocorra qualquer erro, a operação será revertida.
    /// </remarks>
    /// <param name="id">Identificador do pedido.</param>
    /// <response code="204">Pedido confirmado com sucesso.</response>
    /// <response code="404">Pedido ou produto não encontrado.</response>
    /// <response code="409">
    /// Pedido em status inválido ou estoque insuficiente.
    /// </response>
    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ConfirmAsync(
        Guid id,
        CancellationToken ct)
    {
        var result = await orderService.ConfirmAsync(id, ct);

        return result.Match(_ => NoContent());
    }

    /// <summary>
    /// Cancela um pedido.
    /// </summary>
    /// <remarks>
    /// Caso o pedido já esteja confirmado,
    /// o estoque dos produtos será devolvido.
    /// </remarks>
    /// <param name="id">Identificador do pedido.</param>
    /// <response code="204">Pedido cancelado com sucesso.</response>
    /// <response code="404">Pedido ou produto não encontrado.</response>
    /// <response code="409">Pedido em status inválido para cancelamento.</response>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelAsync(
        Guid id,
        CancellationToken ct)
    {
        var result = await orderService.CancelAsync(id, ct);

        return result.Match(_ => NoContent());
    }
}