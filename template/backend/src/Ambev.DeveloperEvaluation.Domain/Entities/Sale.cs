using Ambev.DeveloperEvaluation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations; // Added for potential future use
using System.Linq;
using Ambev.DeveloperEvaluation.Domain.Enums; // Added for SaleStatus

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a Sale aggregate root.
/// </summary>
public class Sale : BaseEntity // Inherit from BaseEntity
{
    // [Key] // Assuming an ID is needed for persistence - Handled by BaseEntity
    // public Guid Id { get; private set; }

    [Required] // Assuming SaleNumber is mandatory
    public string SaleNumber { get; private set; } // Or potentially an int/long? Using string for flexibility.

    public DateTime SaleDate { get; private set; }

    [Required]
    public CustomerInfo Customer { get; private set; }

    [Required]
    public BranchInfo Branch { get; private set; }

    private readonly List<SaleItem> _items = new();
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    public decimal TotalSaleAmount => CalculateTotalSaleAmount();

    public SaleStatus Status { get; private set; }

    // Private constructor for ORM/Serialization
    private Sale() { }

    public Sale(string saleNumber, CustomerInfo customer, BranchInfo branch, IEnumerable<SaleItem> initialItems = null)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(saleNumber)) throw new ArgumentException("Sale number cannot be empty.", nameof(saleNumber));

        Id = Guid.NewGuid(); // Initialize Id from BaseEntity
        SaleNumber = saleNumber;
        SaleDate = DateTime.UtcNow; // Use UTC time
        Customer = customer; // Struct assignment
        Branch = branch; // Struct assignment
        Status = SaleStatus.Active;

        // Add items, enforcing business rules during addition
        foreach (var item in initialItems ?? Enumerable.Empty<SaleItem>())
        {
            AddItem(item.Product, item.Quantity, item.UnitPrice);
        }

        // Raise SaleCreated event (implementation TBD)
        // AddDomainEvent(new SaleCreatedEvent(this));
    }

    // Method to add items, encapsulating rules
    public void AddItem(ProductInfo product, int quantity, decimal unitPrice)
    {
        if (Status == SaleStatus.Cancelled) throw new InvalidOperationException("Cannot modify a cancelled sale.");
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        if (quantity > 20) throw new ArgumentOutOfRangeException(nameof(quantity), "Cannot sell more than 20 identical items.");

        var existingItem = _items.FirstOrDefault(i => i.Product.ProductId == product.ProductId);
        if (existingItem != null)
        {
            // Update quantity of existing item
            int newQuantity = existingItem.Quantity + quantity;
            if (newQuantity > 20) throw new ArgumentOutOfRangeException(nameof(quantity), $"Adding {quantity} units would exceed the limit of 20 for product '{product.Name}'.");
            existingItem.UpdateQuantity(newQuantity);
        }
        else
        {
            // Add new item
            var newItem = new SaleItem(product, quantity, unitPrice);
            _items.Add(newItem);
        }
        // Raise SaleModified event or specific ItemAdded event (implementation TBD)
        // AddDomainEvent(new SaleModifiedEvent(this));
    }

    // Method to modify an existing item's quantity
    public void ModifyItemQuantity(string productId, int newQuantity)
    {
        if (Status == SaleStatus.Cancelled) throw new InvalidOperationException("Cannot modify a cancelled sale.");
        if (newQuantity <= 0) throw new ArgumentOutOfRangeException(nameof(newQuantity), "Quantity must be positive.");
        if (newQuantity > 20) throw new ArgumentOutOfRangeException(nameof(newQuantity), "Quantity cannot exceed 20 units per product.");

        var itemToModify = _items.FirstOrDefault(i => i.Product.ProductId == productId);
        if (itemToModify == null) throw new KeyNotFoundException($"Product with ID '{productId}' not found in sale.");

        itemToModify.UpdateQuantity(newQuantity);

        // Raise SaleModified event (implementation TBD)
        // AddDomainEvent(new SaleModifiedEvent(this));
    }


    // Method to remove an item
    public void RemoveItem(string productId)
    {
        if (Status == SaleStatus.Cancelled) throw new InvalidOperationException("Cannot modify a cancelled sale.");

        var itemToRemove = _items.FirstOrDefault(i => i.Product.ProductId == productId);
        if (itemToRemove != null)
        {
            _items.Remove(itemToRemove);
            // Raise ItemCancelled/Removed event or SaleModified? (Implementation TBD)
            // AddDomainEvent(new ItemRemovedFromSaleEvent(this.Id, productId));
            // AddDomainEvent(new SaleModifiedEvent(this)); // General modification event
        }
        // else: Item not found, maybe log or ignore?
    }


    // Method to cancel the entire sale
    public void CancelSale()
    {
        if (Status == SaleStatus.Cancelled) return; // Already cancelled

        Status = SaleStatus.Cancelled;
        // Optional: Clear items or zero out amounts depending on business rules for cancellation
        // _items.Clear();

        // Raise SaleCancelled event (implementation TBD)
        // AddDomainEvent(new SaleCancelledEvent(this.Id));
    }

    // Recalculate total sale amount
    private decimal CalculateTotalSaleAmount()
    {
        // If cancelled, total might be considered 0 depending on rules
        if (Status == SaleStatus.Cancelled) return 0m;
        return _items.Sum(item => item.TotalItemAmount);
    }

    // Domain event handling would be integrated here if BaseEntity doesn't handle it
    // For example, if BaseEntity had: public List<INotification> DomainEvents { get; }
    // protected void AddDomainEvent(INotification eventItem) => DomainEvents.Add(eventItem);
}
