using FluentAssertions;
using Moq;
using OrderService.Application.DTOs.Orders;
using OrderService.Domain.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Domain.Errors;
using OrderService.Domain.Models;

namespace OrderService.Tests.Unit.Application;

[TestFixture]
public class OrderServiceTests
{
    private Mock<IUnitOfWork> _uow = null!;
    private Mock<IOrderRepository> _orderRepository = null!;
    private Mock<IProductRepository> _productRepository = null!;
    private OrderService.Application.Services.OrderService _service = null!;

    [SetUp]
    public void Setup()
    {
        _uow = new Mock<IUnitOfWork>();
        _orderRepository = new Mock<IOrderRepository>();
        _productRepository = new Mock<IProductRepository>();

        _uow.Setup(x => x.OrderRepository).Returns(_orderRepository.Object);
        _uow.Setup(x => x.ProductRepository).Returns(_productRepository.Object);

        _service = new OrderService.Application.Services.OrderService(_uow.Object);
    }

    #region CreateAsync

    [Test]
    public async Task CreateAsync_ShouldFail_WhenItemsNull()
    {
        var dto = new CreateOrderDto
        {
            CustomerId = 1,
            Currency = "BRL",
            Items = null!
        };

        var result = await _service.CreateAsync(dto, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(OrderErrors.MustContainItems);
    }

    [Test]
    public async Task CreateAsync_ShouldFail_WhenItemsEmpty()
    {
        var dto = new CreateOrderDto
        {
            CustomerId = 1,
            Currency = "BRL",
            Items = []
        };

        var result = await _service.CreateAsync(dto, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(OrderErrors.MustContainItems);
    }

    [Test]
    public async Task CreateAsync_ShouldFail_WhenProductNotFound()
    {
        var dto = CreateDto();

        _productRepository
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await _service.CreateAsync(dto, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Product.NotFound");
    }

    [Test]
    public async Task CreateAsync_ShouldFail_WhenStockInsufficient()
    {
        var dto = CreateDto(quantity: 5);
        var product = CreateProduct(stock: 2);

        _productRepository
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _service.CreateAsync(dto, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Product.InsufficientStock");
    }

    [Test]
    public async Task CreateAsync_ShouldFail_WhenQuantityZero()
    {
        var dto = CreateDto(quantity: 0);
        var product = CreateProduct();

        _productRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _service.CreateAsync(dto, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Product.QuantityMustBeGreaterThanZero");
    }

    [Test]
    public async Task CreateAsync_ShouldFail_WhenQuantityNegative()
    {
        var dto = CreateDto(quantity: -1);
        var product = CreateProduct();

        _productRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _service.CreateAsync(dto, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Product.QuantityMustBeGreaterThanZero");
    }

    [Test]
    public async Task CreateAsync_ShouldCreateOrder_WhenValid()
    {
        var dto = CreateDto(quantity: 2);
        var product = CreateProduct();

        _productRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _service.CreateAsync(dto, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        _orderRepository.Verify(
            x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region ConfirmAsync

    [Test]
    public async Task ConfirmAsync_ShouldFail_WhenOrderNotFound()
    {
        _orderRepository.Setup(x =>
                x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _service.ConfirmAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(OrderErrors.NotFound);
    }

    [Test]
    public async Task ConfirmAsync_ShouldSucceed_WhenAlreadyConfirmed()
    {
        var order = CreateConfirmedOrder();

        _orderRepository.Setup(x =>
                x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _service.ConfirmAsync(order.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task ConfirmAsync_ShouldFail_WhenDraft()
    {
        var order = Order.Create(1, "BRL");

        _orderRepository.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _service.ConfirmAsync(order.Id, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(OrderErrors.InvalidStatusToConfirm);
    }

    [Test]
    public async Task ConfirmAsync_ShouldRollback_WhenProductNotFound()
    {
        var order = CreatePlacedOrder();

        _orderRepository.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await _service.ConfirmAsync(order.Id, CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        _uow.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ConfirmAsync_ShouldRollback_WhenInsufficientStock()
    {
        var order = CreatePlacedOrder(quantity: 10);
        var product = CreateProduct(stock: 2);

        _orderRepository.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _service.ConfirmAsync(order.Id, CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        _uow.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ConfirmAsync_ShouldCommit_WhenSuccess()
    {
        var order = CreatePlacedOrder();
        var product = CreateProduct();

        _orderRepository.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _service.ConfirmAsync(order.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        _uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void ConfirmAsync_ShouldRollback_WhenException()
    {
        var order = CreatePlacedOrder();
        var product = CreateProduct();

        _orderRepository.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _orderRepository.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        Assert.ThrowsAsync<Exception>(async () =>
            await _service.ConfirmAsync(order.Id, CancellationToken.None));

        _uow.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region CancelAsync

    [Test]
    public async Task CancelAsync_ShouldFail_WhenOrderNotFound()
    {
        _orderRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _service.CancelAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public async Task CancelAsync_ShouldSuccess_WhenAlreadyCanceled()
    {
        var order = CreateCanceledOrder();

        _orderRepository.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _service.CancelAsync(order.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task CancelAsync_ShouldCancelPlacedOrder()
    {
        var order = CreatePlacedOrder();

        _orderRepository.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _service.CancelAsync(order.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CancelAsync_ShouldRestoreStock_WhenConfirmed()
    {
        var order = CreateConfirmedOrder();
        var product = CreateProduct();

        _orderRepository.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _service.CancelAsync(order.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        _productRepository.Verify(x =>
            x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void CancelAsync_ShouldRollback_WhenException()
    {
        var order = CreatePlacedOrder();

        _orderRepository.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _orderRepository.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        Assert.ThrowsAsync<Exception>(async () =>
            await _service.CancelAsync(order.Id, CancellationToken.None));

        _uow.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetByIdAsync

    [Test]
    public async Task GetByIdAsync_ShouldFail_WhenOrderNotFound()
    {
        _orderRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(OrderErrors.NotFound);
    }

    [Test]
    public async Task GetByIdAsync_ShouldMapCorrectly()
    {
        var order = CreatePlacedOrder();

        _orderRepository.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _service.GetByIdAsync(order.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);
    }

    #endregion

    #region GetAllAsync

    [Test]
    public async Task GetAllAsync_ShouldReturnEmpty()
    {
        _orderRepository.Setup(x =>
                x.GetAllAsync(null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Order>
            {
                Data = [],
                Page = 1,
                PageSize = 10,
                TotalCount = 0
            });

        var result = await _service.GetAllAsync(null, null, null, null, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Data.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllAsync_ShouldMapPagedResult()
    {
        var order = CreatePlacedOrder();

        _orderRepository.Setup(x =>
                x.GetAllAsync(1, 10, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Order>
            {
                Data = [order],
                Page = 1,
                PageSize = 10,
                TotalCount = 1
            });

        var result = await _service.GetAllAsync(1, 10, null, null, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Data.Should().HaveCount(1);
        result.Data.Page.Should().Be(1);
        result.Data.TotalCount.Should().Be(1);
    }

    #region Advanced Cases

    [Test]
    public async Task CreateAsync_ShouldCreateOrder_WithMultipleItems()
    {
        var dto = new CreateOrderDto
        {
            CustomerId = 1,
            Currency = "BRL",
            Items =
            [
                new CreateOrderItemDto { ProductId = 1, Quantity = 2 },
            new CreateOrderItemDto { ProductId = 2, Quantity = 3 }
            ]
        };

        _productRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProduct(id: 1));

        _productRepository.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProduct(id: 2));

        var result = await _service.CreateAsync(dto, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        _orderRepository.Verify(
            x => x.CreateAsync(It.Is<Order>(o => o.Items.Count == 2), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task CreateAsync_ShouldMergeDuplicatedProducts()
    {
        var dto = new CreateOrderDto
        {
            CustomerId = 1,
            Currency = "BRL",
            Items =
            [
                new CreateOrderItemDto { ProductId = 1, Quantity = 1 },
            new CreateOrderItemDto { ProductId = 1, Quantity = 2 }
            ]
        };

        _productRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProduct());

        Order? captured = null;

        _orderRepository.Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((o, _) => captured = o)
            .Returns(Task.CompletedTask);

        await _service.CreateAsync(dto, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Items.Should().HaveCount(1);
        captured.Items.First().Quantity.Should().Be(3);
    }

    [Test]
    public async Task ConfirmAsync_ShouldUpdateAllProducts_WhenMultipleItems()
    {
        var order = Order.Create(1, "BRL");
        order.AddItem(1, 10, 2);
        order.AddItem(2, 20, 1);
        order.Place();

        _orderRepository.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProduct(1, 50));

        _productRepository.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProduct(2, 50));

        var result = await _service.ConfirmAsync(order.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        _productRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Test]
    public async Task ConfirmAsync_ShouldRollback_WhenSecondProductFails()
    {
        var order = Order.Create(1, "BRL");
        order.AddItem(1, 10, 2);
        order.AddItem(2, 10, 10);
        order.Place();

        _orderRepository.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProduct(1, 100));

        _productRepository.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProduct(2, 1));

        var result = await _service.ConfirmAsync(order.Id, CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        _uow.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ConfirmAsync_ShouldNotUpdateOrder_WhenStockFails()
    {
        var order = CreatePlacedOrder(50);

        _orderRepository.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProduct(stock: 1));

        await _service.ConfirmAsync(order.Id, CancellationToken.None);

        _orderRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task CancelAsync_ShouldNotTouchProductRepository_WhenPlacedOrder()
    {
        var order = CreatePlacedOrder();

        _orderRepository.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await _service.CancelAsync(order.Id, CancellationToken.None);

        _productRepository.Verify(
            x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task CancelAsync_ShouldRollback_WhenProductNotFound()
    {
        var order = CreateConfirmedOrder();

        _orderRepository.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await _service.CancelAsync(order.Id, CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        _uow.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-100)]
    public async Task Product_RemoveStock_ShouldFail_WithInvalidQuantity(int qty)
    {
        var product = CreateProduct();

        var result = product.RemoveStock(qty);

        result.IsFailure.Should().BeTrue();
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-100)]
    public void Product_AddStock_ShouldFail_WithInvalidQuantity(int qty)
    {
        var product = CreateProduct();

        var result = product.AddStock(qty);

        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public async Task ConfirmAsync_ShouldCallTransactionMethods_InOrder()
    {
        var order = CreatePlacedOrder();
        var product = CreateProduct();

        var sequence = new MockSequence();

        _orderRepository.InSequence(sequence)
            .Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _uow.InSequence(sequence)
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _productRepository.InSequence(sequence)
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _productRepository.InSequence(sequence)
            .Setup(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _orderRepository.InSequence(sequence)
            .Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _uow.InSequence(sequence)
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.ConfirmAsync(order.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task GetAllAsync_ShouldPreserveFilters()
    {
        DateTime start = DateTime.UtcNow.AddDays(-5);
        DateTime end = DateTime.UtcNow;

        _orderRepository.Setup(x =>
            x.GetAllAsync(1, 10, start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Order>
            {
                Data = [],
                Page = 1,
                PageSize = 10,
                TotalCount = 0
            });

        await _service.GetAllAsync(1, 10, start, end, CancellationToken.None);

        _orderRepository.Verify(
            x => x.GetAllAsync(1, 10, start, end, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #endregion

    private static Product CreateProduct(
        int id = 1,
        int stock = 100,
        decimal price = 10)
    {
        return new Product
        {
            Id = id,
            Name = "Product",
            UnitPrice = price,
            AvailableStock = stock
        };
    }

    private static CreateOrderDto CreateDto(int quantity = 1)
    {
        return new CreateOrderDto
        {
            CustomerId = 1,
            Currency = "BRL",
            Items =
            [
                new CreateOrderItemDto
                {
                    ProductId = 1,
                    Quantity = quantity
                }
            ]
        };
    }

    private static Order CreatePlacedOrder(int quantity = 2)
    {
        var order = Order.Create(1, "BRL");
        order.AddItem(1, 10, quantity);
        order.Place();
        return order;
    }

    private static Order CreateConfirmedOrder()
    {
        var order = CreatePlacedOrder();
        order.Confirm();
        return order;
    }

    private static Order CreateCanceledOrder()
    {
        var order = CreatePlacedOrder();
        order.Cancel();
        return order;
    }
}