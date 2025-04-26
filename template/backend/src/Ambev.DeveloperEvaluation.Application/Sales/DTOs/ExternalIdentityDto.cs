using System.ComponentModel.DataAnnotations;

namespace Ambev.DeveloperEvaluation.Application.Sales.DTOs;

/// <summary>
/// DTO for ExternalIdentity.
/// </summary>
public record ExternalIdentityDto
{
    [Required]
    public string Id { get; init; }

    [Required]
    public string Description { get; init; }
}
