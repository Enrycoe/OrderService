using FluentAssertions;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Tests.Unit.Domain;

[TestFixture]
public class OrderTests
{
    [Test]
    public void Create_ShouldCreateDraftOrder()
    {
        var order = Order.Create(123, "BRL");

        order.Should().NotBeNull();
        order.Id.Should().NotBeEmpty();
        order.CustomerId.Should().Be(123);
        order.Currency.Should().Be("BRL");
        order.Status.Should().Be(OrderStatus.Draft);
        order.Items.Should().BeEmpty();
        order.Total.Should().Be(0);
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #region AddItem

    [Test]
    public void AddItem_ShouldAddNewItem()
    {
        var order = Order.Create(1, "BRL");

        var result = order.AddItem(10, 50, 2);

        result.IsSuccess.Should().BeTrue();

        order.Items.Should().HaveCount(1);
        order.Total.Should().Be(100);
    }

    [Test]
    public void AddItem_ShouldFail_WhenQuantityIsZero()
    {
        var order = Order.Create(1, "BRL");

        var result = order.AddItem(10, 10, 0);

        result.IsFailure.Should().BeTrue();
        order.Items.Should().BeEmpty();
    }

    [Test]
    public void AddItem_ShouldFail_WhenQuantityIsNegative()
    {
        var order = Order.Create(1, "BRL");

        var result = order.AddItem(10, 10, -1);

        result.IsFailure.Should().BeTrue();
        order.Items.Should().BeEmpty();
    }

    [Test]
    public void AddItem_ShouldFail_WhenPriceIsZero()
    {
        var order = Order.Create(1, "BRL");

        var result = order.AddItem(10, 0, 1);

        result.IsFailure.Should().BeTrue();
        order.Items.Should().BeEmpty();
    }

    [Test]
    public void AddItem_ShouldFail_WhenPriceIsNegative()
    {
        var order = Order.Create(1, "BRL");

        var result = order.AddItem(10, -10, 1);

        result.IsFailure.Should().BeTrue();
        order.Items.Should().BeEmpty();
    }

    [Test]
    public void AddItem_ShouldMergeDuplicatedProducts()
    {
        var order = Order.Create(1, "BRL");

        order.AddItem(10, 10, 1);
        order.AddItem(10, 10, 2);

        order.Items.Should().HaveCount(1);
        order.Items.Single().Quantity.Should().Be(3);
    }

    [Test]
    public void AddItem_ShouldRecalculateTotal()
    {
        var order = Order.Create(1, "BRL");

        order.AddItem(1, 10, 2);
        order.AddItem(2, 20, 3);

        order.Total.Should().Be(80);
    }

    [Test]
    public void AddItem_ShouldAllowDifferentProducts()
    {
        var order = Order.Create(1, "BRL");

        order.AddItem(1, 10, 1);
        order.AddItem(2, 20, 1);

        order.Items.Should().HaveCount(2);
    }

    #endregion

    #region Place

    [Test]
    public void Place_ShouldChangeStatusToPlaced()
    {
        var order = Order.Create(1, "BRL");

        order.Place();

        order.Status.Should().Be(OrderStatus.Placed);
    }

    #endregion

    #region Confirm

    [Test]
    public void Confirm_ShouldConfirmPlacedOrder()
    {
        var order = CreatePlacedOrder();

        var result = order.Confirm();

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Test]
    public void Confirm_ShouldReturnSuccess_WhenAlreadyConfirmed()
    {
        var order = CreatePlacedOrder();
        order.Confirm();

        var result = order.Confirm();

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void Confirm_ShouldFail_WhenDraft()
    {
        var order = Order.Create(1, "BRL");

        var result = order.Confirm();

        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public void Confirm_ShouldFail_WhenCanceled()
    {
        var order = CreatePlacedOrder();

        order.Cancel();

        var result = order.Confirm();

        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public void Confirm_ShouldFail_WhenPlacedOrderHasNoItems()
    {
        var order = Order.Create(1, "BRL");

        order.Place();

        var result = order.Confirm();

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Cancel

    [Test]
    public void Cancel_ShouldCancelPlacedOrder()
    {
        var order = CreatePlacedOrder();

        var result = order.Cancel();

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Canceled);
    }

    [Test]
    public void Cancel_ShouldCancelConfirmedOrder()
    {
        var order = CreatePlacedOrder();

        order.Confirm();

        var result = order.Cancel();

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Canceled);
    }

    [Test]
    public void Cancel_ShouldReturnSuccess_WhenAlreadyCanceled()
    {
        var order = CreatePlacedOrder();

        order.Cancel();

        var result = order.Cancel();

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void Cancel_ShouldFail_WhenDraft()
    {
        var order = Order.Create(1, "BRL");

        var result = order.Cancel();

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    private static Order CreatePlacedOrder()
    {
        var order = Order.Create(1, "BRL");
        order.AddItem(1, 10, 2);
        order.Place();

        return order;
    }
}
