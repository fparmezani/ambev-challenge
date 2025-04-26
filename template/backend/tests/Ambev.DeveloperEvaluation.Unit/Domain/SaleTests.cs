using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Xunit; // Using xUnit

namespace Ambev.DeveloperEvaluation.Unit.Domain;

public class SaleTests
{
    // --- Helper Methods/Data ---
    private CustomerInfo CreateDummyCustomer() => new("CUST-001", "Test Customer");
    private BranchInfo CreateDummyBranch() => new("BRANCH-001", "Test Branch");
    private ProductInfo CreateDummyProduct(string id = "PROD-001", string desc = "Test Product") => new(id, desc,"");
    private Sale CreateSaleWithItems(string saleNumber, List<SaleItem> items) =>
        new Sale(saleNumber, CreateDummyCustomer(), CreateDummyBranch(), items);
    private Sale CreateBasicSale(string saleNumber = "SALE-123") =>
        new Sale(saleNumber, CreateDummyCustomer(), CreateDummyBranch(), new List<SaleItem>());


    // --- Test Cases ---

    [Fact]
    public void Sale_Constructor_ShouldSetInitialPropertiesCorrectly()
    {
        // Arrange
        var saleNumber = "S-001";
        var customer = CreateDummyCustomer();
        var branch = CreateDummyBranch();
        var product = CreateDummyProduct();
        var items = new List<SaleItem> { new SaleItem(product, 5, 10.0m) }; // 5 items should get 10% discount

        // Act
        var sale = new Sale(saleNumber, customer, branch, items);

        // Assert
        Assert.NotEqual(Guid.Empty, sale.Id);
        Assert.Equal(saleNumber, sale.SaleNumber);
        Assert.Equal(customer, sale.Customer);
        Assert.Equal(branch, sale.Branch);
        Assert.Single(sale.Items);
        Assert.InRange(sale.SaleDate, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1)); // Check if date is recent UTC
    }

    [Fact]
    public void Sale_AddItem_ShouldAddValidItemAndCalculateTotal()
    {
        // Arrange
        var sale = CreateBasicSale();
        var product1 = CreateDummyProduct("P1", "Prod 1");
        var product2 = CreateDummyProduct("P2", "Prod 2");

        // Act
        sale.AddItem(product1, 3, 10.0m); // No discount
        sale.AddItem(product2, 5, 20.0m); // 10% discount

        // Assert
        Assert.Equal(2, sale.Items.Count);

        var item1 = sale.Items.First(i => i.Product.ProductId == "P1");
        Assert.Equal(0m, item1.Discount);
        Assert.Equal(3 * 10.0m, item1.TotalItemAmount);

        var item2 = sale.Items.First(i => i.Product.ProductId == "P2");
        Assert.Equal(0.10m, item2.Discount);
        Assert.Equal((5 * 20.0m) * (1 - 0.10m), item2.TotalItemAmount);

        Assert.Equal(item1.TotalItemAmount + item2.TotalItemAmount, sale.TotalSaleAmount);
    }

    [Theory]
    [InlineData(0)]  // Below min limit
    [InlineData(-1)] // Negative
    public void Sale_AddItem_ShouldThrowWhenQuantityIsInvalid(int invalidQuantity)
    {
        // Arrange
        var sale = CreateBasicSale();
        var product = CreateDummyProduct();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => sale.AddItem(product, invalidQuantity, 10.0m));
    }


    [Fact]
    public void Sale_AddItem_ShouldThrowWhenQuantityExceedsLimit()
    {
        // Arrange
        var sale = CreateBasicSale();
        var product = CreateDummyProduct();
        var quantityOverLimit = 21;

        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sale.AddItem(product, quantityOverLimit, 10.0m));
        Assert.Contains("Cannot sell more than 20 identical items", ex.Message);
    }

    [Theory]
    [InlineData(1, 0)]    // Qty 1 -> 0% discount
    [InlineData(3, 0)]    // Qty 3 -> 0% discount
    [InlineData(4, 0.10)] // Qty 4 -> 10% discount
    [InlineData(9, 0.10)] // Qty 9 -> 10% discount
    [InlineData(10, 0.20)]// Qty 10 -> 20% discount
    [InlineData(15, 0.20)]// Qty 15 -> 20% discount
    [InlineData(20, 0.20)]// Qty 20 -> 20% discount
    public void SaleItem_DiscountCalculation_ShouldBeCorrectBasedOnQuantity(int quantity, decimal expectedDiscount)
    {
        // Arrange
        var product = CreateDummyProduct();
        var unitPrice = 100m;

        // Act
        var saleItem = new SaleItem(product, quantity, unitPrice);

        // Assert
        Assert.Equal(expectedDiscount, saleItem.Discount);
        Assert.Equal((quantity * unitPrice) * (1 - expectedDiscount), saleItem.TotalItemAmount);
    }


    [Fact]
    public void Sale_CancelSale_ShouldSetIsCancelledFlag()
    {
        // Arrange
        var sale = CreateBasicSale();
        sale.AddItem(CreateDummyProduct(), 5, 10m);

        // Act
        sale.CancelSale();

        // Assert
        Assert.True(sale.Status == DeveloperEvaluation.Domain.Enums.SaleStatus.Cancelled);
    }

    [Fact]
    public void Sale_ModifyItemQuantity_ShouldUpdateQuantityAndDiscount()
    {
        // Arrange
        var sale = CreateBasicSale();
        var product = CreateDummyProduct();
        sale.AddItem(product, 3, 100m); // Initially 0% discount

        // Act
        sale.ModifyItemQuantity(product.ProductId, 5); // Change quantity to trigger 10% discount

        // Assert
        var updatedItem = sale.Items.First(i => i.Product.ProductId == product.ProductId);
        Assert.Equal(5, updatedItem.Quantity);
        Assert.Equal(0.10m, updatedItem.Discount);
        Assert.Equal((5 * 100m) * (1 - 0.10m), updatedItem.TotalItemAmount);
        Assert.Equal(updatedItem.TotalItemAmount, sale.TotalSaleAmount); // Total should update
    }

    

    [Fact]
    public void Sale_ModifyItemQuantity_ShouldThrowOnCancelledSale()
    {
        // Arrange
        var sale = CreateBasicSale();
        var product = CreateDummyProduct();
        sale.AddItem(product, 5, 10m);
        sale.CancelSale(); // Cancel the sale

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => sale.ModifyItemQuantity(product.ProductId, 10));
        Assert.Contains("Cannot modify a cancelled sale", ex.Message);
    }


    [Fact]
    public void Sale_RemoveItem_ShouldRemoveItemFromCollection()
    {
        // Arrange
        var sale = CreateBasicSale();
        var product1 = CreateDummyProduct("P1");
        var product2 = CreateDummyProduct("P2");
        sale.AddItem(product1, 5, 10m);
        sale.AddItem(product2, 2, 20m);
        var initialTotal = sale.TotalSaleAmount;

        // Act
        sale.RemoveItem(product1.ProductId); // Remove P1

        // Assert
        Assert.Single(sale.Items);
        Assert.Null(sale.Items.FirstOrDefault(i => i.Product.ProductId == "P1"));
        Assert.NotNull(sale.Items.FirstOrDefault(i => i.Product.ProductId == "P2"));
        Assert.NotEqual(initialTotal, sale.TotalSaleAmount); // Total should update
        Assert.Equal(sale.Items.First().TotalItemAmount, sale.TotalSaleAmount);
    }

     [Fact]
    public void Sale_RemoveItem_ShouldThrowOnCancelledSale()
    {
        // Arrange
        var sale = CreateBasicSale();
        var product = CreateDummyProduct();
        sale.AddItem(product, 5, 10m);
        sale.CancelSale(); // Cancel the sale

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => sale.RemoveItem(product.ProductId));
        Assert.Contains("Cannot modify a cancelled sale", ex.Message);
    }
}
