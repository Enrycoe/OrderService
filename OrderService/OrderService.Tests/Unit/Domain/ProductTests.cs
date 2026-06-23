using FluentAssertions;
using OrderService.Domain.Entities;

namespace OrderService.Tests.Unit.Domain;

[TestFixture]
public class ProductTests
{
    #region RemoveStock

    [Test]
    public void RemoveStock_ShouldDecreaseStock()
    {
        var product = CreateProduct(stock: 10);

        var result = product.RemoveStock(3);

        result.IsSuccess.Should().BeTrue();
        product.AvailableStock.Should().Be(7);
    }

    [Test]
    public void RemoveStock_ShouldAllowRemovingExactStock()
    {
        var product = CreateProduct(stock: 10);

        var result = product.RemoveStock(10);

        result.IsSuccess.Should().BeTrue();
        product.AvailableStock.Should().Be(0);
    }

    [Test]
    public void RemoveStock_ShouldFail_WhenStockIsInsufficient()
    {
        var product = CreateProduct(stock: 5);

        var result = product.RemoveStock(6);

        result.IsFailure.Should().BeTrue();
        product.AvailableStock.Should().Be(5);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-10)]
    public void RemoveStock_ShouldFail_WhenQuantityIsInvalid(int quantity)
    {
        var product = CreateProduct(stock: 10);

        var result = product.RemoveStock(quantity);

        result.IsFailure.Should().BeTrue();
        product.AvailableStock.Should().Be(10);
    }

    #endregion

    #region AddStock

    [Test]
    public void AddStock_ShouldIncreaseStock()
    {
        var product = CreateProduct(stock: 10);

        var result = product.AddStock(5);

        result.IsSuccess.Should().BeTrue();
        product.AvailableStock.Should().Be(15);
    }

    [Test]
    public void AddStock_ShouldIncreaseStockMultipleTimes()
    {
        var product = CreateProduct(stock: 10);

        product.AddStock(5);
        product.AddStock(10);

        product.AvailableStock.Should().Be(25);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-100)]
    public void AddStock_ShouldFail_WhenQuantityIsInvalid(int quantity)
    {
        var product = CreateProduct(stock: 10);

        var result = product.AddStock(quantity);

        result.IsFailure.Should().BeTrue();
        product.AvailableStock.Should().Be(10);
    }

    #endregion

    private static Product CreateProduct(
        int id = 1,
        int stock = 10,
        decimal price = 100)
    {
        return new Product
        {
            Id = id,
            Name = "Product",
            UnitPrice = price,
            AvailableStock = stock
        };
    }
}
