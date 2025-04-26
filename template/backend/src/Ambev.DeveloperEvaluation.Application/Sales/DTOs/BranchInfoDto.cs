using System.ComponentModel.DataAnnotations;

namespace Ambev.DeveloperEvaluation.Application.Sales.DTOs;

/// <summary>
/// DTO for Branch information (denormalized).
/// </summary>
public record BranchInfoDto
{
    [Required]
    public string BranchId { get; init; }

    [Required]
    public string Name { get; init; }
}
