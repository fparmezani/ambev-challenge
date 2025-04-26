using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums; // Add for SaleStatus
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleMap : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales"); // Explicitly set table name

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SaleNumber)
               .IsRequired()
               .HasMaxLength(50); // Example max length, adjust as needed

        builder.Property(s => s.SaleDate)
               .IsRequired();

        builder.Property(s => s.TotalSaleAmount) // Map the calculated property
               .HasColumnType("decimal(18,2)"); // Define precision

        builder.Property(s => s.Status)
               .IsRequired()
               .HasConversion<string>() // Store as string
               .HasMaxLength(20); // Max length for enum string (e.g., "Cancelled")

        // Configure CustomerInfo as an owned entity (maps to columns in Sales table)
        builder.OwnsOne(s => s.Customer, customer =>
        {
            customer.Property(c => c.CustomerId)
                    .HasColumnName("CustomerId") // Define column name
                    .IsRequired()
                    .HasMaxLength(100);
            customer.Property(c => c.Name)
                    .HasColumnName("CustomerName") // Define column name
                    .IsRequired()
                    .HasMaxLength(255);
        });

        // Configure BranchInfo as an owned entity
        builder.OwnsOne(s => s.Branch, branch =>
        {
            branch.Property(b => b.BranchId)
                  .HasColumnName("BranchId")
                  .IsRequired()
                  .HasMaxLength(100);
            branch.Property(b => b.Name)
                  .HasColumnName("BranchName")
                  .IsRequired()
                  .HasMaxLength(255);
        });

        // Configure the relationship with SaleItem as a collection of owned entities
        // This maps SaleItems to a separate "SaleItems" table linked back to "Sales"
        builder.OwnsMany(s => s.Items, item =>
        {
            item.WithOwner().HasForeignKey("SaleId"); // Define the foreign key
            item.ToTable("SaleItems"); // Explicitly set table name

            item.Property<Guid>("SaleId"); // Define shadow property for FK if not explicit in SaleItem
            item.Property<string>("ProductId"); // Define shadow property for the ProductId part of the key
            item.HasKey("SaleId", "ProductId"); // Use shadow properties for composite key

            item.Property(i => i.Quantity)
                .IsRequired();

            item.Property(i => i.UnitPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            item.Property(i => i.Discount)
                .HasColumnType("decimal(3,2)") // e.g., 0.10, 0.20
                .IsRequired();

            item.Property(i => i.TotalItemAmount) // Map calculated property
                .HasColumnType("decimal(18,2)");

            // Configure ProductInfo within SaleItem as an owned entity (Value Object)
            item.OwnsOne(i => i.Product, product =>
            {
                // Map ProductId to the column used in the composite key
                product.Property(p => p.ProductId)
                       .HasColumnName("ProductId")
                       .IsRequired()
                       .HasMaxLength(100);
                // Map other ProductInfo properties
                product.Property(p => p.Name)
                       .HasColumnName("ProductName")
                       .IsRequired()
                       .HasMaxLength(255);
                product.Property(p => p.Description)
                       .HasColumnName("ProductDescription")
                       .HasMaxLength(500); // Allow longer description
            });

            // Ignore the TotalItemAmount property for direct mapping as it's calculated
            // EF Core 6+ might handle this better automatically, but explicitly ignoring ensures it.
            // Update: EF Core can map properties with private setters and backing fields,
            // but mapping calculated properties requires care. Let's rely on the getter.
            // If issues arise, mapping it as `.HasComputedColumnSql(...)` might be needed if DB generates it,
            // or just ensure it's calculated correctly in the entity.
            // For owned types, calculated properties are generally fine as long as underlying properties are mapped.
        });

        // Define navigation property access mode if needed (e.g., if using backing field directly)
        var navigation = builder.Metadata.FindNavigation(nameof(Sale.Items));
        navigation?.SetPropertyAccessMode(PropertyAccessMode.Field); // Use the private _items field
    }
}
