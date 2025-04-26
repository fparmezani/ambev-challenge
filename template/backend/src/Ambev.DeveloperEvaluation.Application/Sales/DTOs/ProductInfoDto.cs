using System.ComponentModel.DataAnnotations;

namespace Ambev.DeveloperEvaluation.Application.Sales.DTOs;

/// <summary>
/// DTO for Product information (denormalized).
/// </summary>
public record ProductInfoDto
{
    [Required]
    public string ProductId { get; init; }

    [Required]
    public string Name { get; init; }

    // Description might be optional in the DTO depending on usage
    public string Description { get; init; }
}
