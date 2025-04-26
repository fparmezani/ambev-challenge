using Ambev.DeveloperEvaluation.Domain.Common;
using System.ComponentModel.DataAnnotations; // Added for potential future use

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents an item within a sale.
/// </summary>
public class SaleItem
{
    [Required] // Assuming Product is required for a sale item
    public ProductInfo Product { get; private set; }

    [Range(1, 20)] // Quantity must be between 1 and 20 (inclusive)
    public int Quantity { get; private set; }

    [Range(0, double.MaxValue)] // Unit price cannot be negative
    public decimal UnitPrice { get; private set; }

    [Range(0, 1)] // Discount as a percentage (0 to 1)
    public decimal Discount { get; private set; }

    public decimal TotalItemAmount => CalculateTotalItemAmount();

    // Private constructor for ORM/Serialization
    private SaleItem() {}

    public SaleItem(ProductInfo product, int quantity, decimal unitPrice)
    {
        // Business rule validation
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        if (quantity > 20) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot exceed 20 units per product.");
        if (unitPrice < 0) throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");

        Product = product; // ProductInfo is a struct, direct assignment is fine.
        Quantity = quantity;
        UnitPrice = unitPrice;
        Discount = CalculateDiscount(quantity); // Calculate initial discount
    }

    // Method to update quantity and recalculate discount
    internal void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0) throw new ArgumentOutOfRangeException(nameof(newQuantity), "Quantity must be positive.");
        if (newQuantity > 20) throw new ArgumentOutOfRangeException(nameof(newQuantity), "Quantity cannot exceed 20 units per product.");

        Quantity = newQuantity;
        UpdateDiscount(); // Recalculate discount based on new quantity
    }

    // Internal method to allow Sale aggregate to update discount if needed (e.g., during modification)
    // Or this calculation could live entirely within the Sale aggregate when adding/updating items.
    internal void UpdateDiscount()
    {
        Discount = CalculateDiscount(Quantity);
    }

    // Encapsulated discount logic based on business rules
    private decimal CalculateDiscount(int quantity)
    {
        if (quantity >= 10 && quantity <= 20) return 0.20m; // 20% discount
        if (quantity >= 4) return 0.10m; // 10% discount
        return 0m; // No discount
    }

    // Encapsulated total calculation logic
    private decimal CalculateTotalItemAmount()
    {
        return (Quantity * UnitPrice) * (1 - Discount);
    }

    // Potential method for cancelling an item (if needed as separate action)
    // public void CancelItem() { /* Logic TBD - maybe sets a flag? */ }
}
