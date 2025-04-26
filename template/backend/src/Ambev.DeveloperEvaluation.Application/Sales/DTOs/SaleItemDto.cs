using System.ComponentModel.DataAnnotations;

namespace Ambev.DeveloperEvaluation.Application.Sales.DTOs;

/// <summary>
/// DTO for SaleItem. Used for both request creation and response.
/// </summary>
public record SaleItemDto
{
    [Required]
    public ProductInfoDto Product { get; init; }

    [Required]
    [Range(1, 20)] // Enforce max quantity rule at DTO level too
    public int Quantity { get; init; }

    [Required]
    [Range(0.01, (double)decimal.MaxValue)] // Price must be positive
    public decimal UnitPrice { get; init; }

    // Response-only fields (calculated)
    public decimal Discount { get; init; } // Calculated in domain, mapped back
    public decimal TotalItemAmount { get; init; } // Calculated in domain, mapped back
}
