using System.ComponentModel.DataAnnotations;

namespace Ambev.DeveloperEvaluation.Application.Sales.DTOs;

/// <summary>
/// DTO for Customer information (denormalized).
/// </summary>
public record CustomerInfoDto
{
    [Required]
    public string CustomerId { get; init; }

    [Required]
    public string Name { get; init; }
}
