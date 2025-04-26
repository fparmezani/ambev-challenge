using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ambev.DeveloperEvaluation.Application.Sales.DTOs;

/// <summary>
/// DTO for creating a new Sale.
/// </summary>
public class CreateSaleRequest
{
    [Required]
    [MinLength(1)] // Ensure at least one character
    public string SaleNumber { get; set; }

    [Required]
    public ExternalIdentityDto Customer { get; set; }

    [Required]
    public ExternalIdentityDto Branch { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one sale item is required.")]
    public List<SaleItemDto> Items { get; set; } = new List<SaleItemDto>();
}
