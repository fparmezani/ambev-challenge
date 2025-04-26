using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Bogus;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities
{
    public class SaleTests
    {
        private readonly Faker _faker = new("en_US"); // Use Bogus for fake data

        // Helper method to create a valid ProductInfo
        private ProductInfo CreateFakeProductInfo(string? productId = null)
        {
            return new ProductInfo(
                productId ?? _faker.Commerce.Ean13(),
                _faker.Commerce.ProductName(),
                _faker.Commerce.ProductDescription()
            );
        }

        // Helper method to create a valid SaleItem
        private SaleItem CreateFakeSaleItem(int? quantity = null, decimal? unitPrice = null, string? productId = null)
        {
             var product = CreateFakeProductInfo(productId);
             return new SaleItem(
                 product,
                 quantity ?? _faker.Random.Int(1, 20),
                 unitPrice ?? _faker.Random.Decimal(1, 1000)
             );
        }

        // Helper method to create CustomerInfo
        private CustomerInfo CreateFakeCustomerInfo()
        {
            return new CustomerInfo(_faker.Random.Guid().ToString(), _faker.Person.FullName);
        }

         // Helper method to create BranchInfo
        private BranchInfo CreateFakeBranchInfo()
        {
            return new BranchInfo(_faker.Random.Guid().ToString(), _faker.Company.CompanyName());
        }

        [Fact]
        public void Constructor_ShouldCreateSale_WithValidParameters()
        {
            // Arrange
            var saleNumber = _faker.Random.AlphaNumeric(10);
            var customer = CreateFakeCustomerInfo();
            var branch = CreateFakeBranchInfo();
            var items = new List<SaleItem> { CreateFakeSaleItem(quantity: 5), CreateFakeSaleItem(quantity: 10) };
            var expectedTotalAmount = items.Sum(i => i.TotalItemAmount);

            // Act
            var sale = new Sale(saleNumber, customer, branch, items);

            // Assert
            sale.Should().NotBeNull();
            sale.Id.Should().NotBeEmpty();
            sale.SaleNumber.Should().Be(saleNumber);
            sale.Customer.Should().Be(customer);
            sale.Branch.Should().Be(branch);
            sale.Status.Should().Be(SaleStatus.Active);
            sale.Items.Should().HaveCount(2);
            sale.Items.Should().BeEquivalentTo(items); // Checks content equality
            sale.TotalSaleAmount.Should().Be(expectedTotalAmount);
            sale.SaleDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5)); // Check creation date
        }

        [Fact]
        public void AddItem_ShouldAddNewItem_WhenProductDoesNotExist()
        {
            // Arrange
            var sale = new Sale(_faker.Random.AlphaNumeric(10), CreateFakeCustomerInfo(), CreateFakeBranchInfo(), new List<SaleItem>());
            var newItem = CreateFakeSaleItem(quantity: 3);
            var initialTotal = sale.TotalSaleAmount;

            // Act
            sale.AddItem(newItem.Product, newItem.Quantity, newItem.UnitPrice);

            // Assert
            sale.Items.Should().HaveCount(1);
            sale.Items.Should().ContainEquivalentOf(newItem);
            sale.TotalSaleAmount.Should().Be(initialTotal + newItem.TotalItemAmount);
        }

        [Fact]
        public void AddItem_ShouldUpdateQuantity_WhenProductAlreadyExists()
        {
            // Arrange
            var existingProduct = CreateFakeProductInfo();
            var initialItem = new SaleItem(existingProduct, 5, 10m); // Qty=5
            var sale = new Sale(_faker.Random.AlphaNumeric(10), CreateFakeCustomerInfo(), CreateFakeBranchInfo(), new List<SaleItem>());
            sale.AddItem(initialItem.Product, initialItem.Quantity, initialItem.UnitPrice);
            var itemToAdd = new SaleItem(existingProduct, 3, 10m); // Qty=3, same product
            var expectedNewQuantity = 8;
            var expectedTotal = initialItem.TotalItemAmount + itemToAdd.TotalItemAmount; // Calculate expected total based on individual items

            // Act
            sale.AddItem(itemToAdd.Product, itemToAdd.Quantity, itemToAdd.UnitPrice);

            // Assert
            sale.Items.Should().HaveCount(1);
            var updatedItem = sale.Items.First(i => i.Product.ProductId == existingProduct.ProductId);
            updatedItem.Quantity.Should().Be(expectedNewQuantity);
            // Total amount should be recalculated based on new quantity and potential discount change
            updatedItem.TotalItemAmount.Should().Be(expectedNewQuantity * 10m * (1 - updatedItem.Discount));
            sale.TotalSaleAmount.Should().Be(updatedItem.TotalItemAmount);
        }

        
        [Fact]
        public void AddItem_ShouldThrowInvalidOperationException_WhenSaleIsCancelled()
        {
            // Arrange
            var product = CreateFakeProductInfo(); // Use helper
            var sale = new Sale(_faker.Random.AlphaNumeric(10), CreateFakeCustomerInfo(), CreateFakeBranchInfo());
            sale.AddItem(product, 10, 5m); // Add initial item
            sale.CancelSale();

            // Act
            Action act = () => sale.AddItem(product, 1, 5m); // Try to add another

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Cannot modify a cancelled sale.");
        }

        
        
        [Theory]
        [InlineData(0)]  // Test quantity <= 0
        [InlineData(21)] // Test quantity > 20
        public void ModifyItemQuantity_ShouldThrowArgumentOutOfRangeException_WhenQuantityIsInvalid(int invalidQuantity)
        {
            // Arrange
            var product = CreateFakeProductInfo(); // Use helper
            var sale = new Sale(_faker.Random.AlphaNumeric(10), CreateFakeCustomerInfo(), CreateFakeBranchInfo());
            sale.AddItem(product, 10, 5m); // Add initial item

            // Act & Assert for quantity <= 0
            Action actZero = () => sale.ModifyItemQuantity(product.ProductId, 0); // Use ProductId
            actZero.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("newQuantity");

            // Act & Assert for quantity > 20
            Action actOver = () => sale.ModifyItemQuantity(product.ProductId, 21); // Use ProductId
            actOver.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("newQuantity");
        }

        [Fact]
        public void ModifyItemQuantity_ShouldThrowInvalidOperationException_WhenSaleIsCancelled()
        {
            // Arrange
            var product = CreateFakeProductInfo(); // Use helper
            var sale = new Sale(_faker.Random.AlphaNumeric(10), CreateFakeCustomerInfo(), CreateFakeBranchInfo());
            sale.AddItem(product, 10, 5m); // Add initial item
            sale.CancelSale();

            // Act
            Action act = () => sale.ModifyItemQuantity(product.ProductId, 5); // Use ProductId

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Cannot modify a cancelled sale.");
        }

        [Fact]
        public void RemoveItem_ShouldRemoveItemAndAdjustTotal_WhenItemExists()
        {
            // Arrange
            var productToRemove = CreateFakeProductInfo(productId: "REMOVE_ME"); // Use helper
            var itemToRemove = new SaleItem(productToRemove, 5, 10m);
            var otherItem = CreateFakeSaleItem(quantity: 3);
            var sale = new Sale(_faker.Random.AlphaNumeric(10), CreateFakeCustomerInfo(), CreateFakeBranchInfo());
            sale.AddItem(productToRemove, 5, 10m); // Add item to remove
            sale.AddItem(otherItem.Product, otherItem.Quantity, otherItem.UnitPrice); // Add other item
            var initialTotal = sale.TotalSaleAmount;
            var initialCount = sale.Items.Count;

            // Act
            sale.RemoveItem(productToRemove.ProductId); // Use ProductId

            // Assert
            sale.Items.Should().HaveCount(initialCount - 1);
            sale.Items.Should().NotContain(i => i.Product.ProductId == productToRemove.ProductId);
            sale.Items.Should().ContainEquivalentOf(otherItem);
            sale.TotalSaleAmount.Should().Be(otherItem.TotalItemAmount);
            sale.TotalSaleAmount.Should().BeLessThan(initialTotal);
        }

        [Fact]
        public void RemoveItem_ShouldRemoveLastItemAndSetTotalToZero_WhenRemovingLastItem()
        {
            // Arrange
            var productToRemove = CreateFakeProductInfo(productId: "REMOVE_ME"); // Use helper
            var itemToRemove = new SaleItem(productToRemove, 5, 10m);
            var sale = new Sale(_faker.Random.AlphaNumeric(10), CreateFakeCustomerInfo(), CreateFakeBranchInfo());
            sale.AddItem(productToRemove, 5, 10m); // Add item to remove

            // Act
            sale.RemoveItem(productToRemove.ProductId); // Use ProductId

            // Assert
            sale.Items.Should().BeEmpty();
            sale.TotalSaleAmount.Should().Be(0);
        }

        
        [Fact]
        public void RemoveItem_ShouldThrowInvalidOperationException_WhenSaleIsCancelled()
        {
            // Arrange
            var productToRemove = CreateFakeProductInfo(productId: "REMOVE_ME"); // Use helper
            var initialItem = new SaleItem(productToRemove, 10, 5m);
            var sale = new Sale(_faker.Random.AlphaNumeric(10), CreateFakeCustomerInfo(), CreateFakeBranchInfo());
            sale.AddItem(productToRemove, 10, 5m); // Add item to remove
            sale.CancelSale();

            // Act
            Action act = () => sale.RemoveItem(productToRemove.ProductId); // Use ProductId

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Cannot modify a cancelled sale.");
        }

        [Fact]
        public void CancelSale_ShouldSetStatusToCancelled_WhenSaleIsActive()
        {
            // Arrange
            var sale = new Sale(_faker.Random.AlphaNumeric(10), CreateFakeCustomerInfo(), CreateFakeBranchInfo(), new List<SaleItem> { CreateFakeSaleItem() });
            sale.Status.Should().Be(SaleStatus.Active);

            // Act
            sale.CancelSale();

            // Assert
            sale.Status.Should().Be(SaleStatus.Cancelled);
        }

        // --- Add more tests for other business rules etc. ---
    }
}
