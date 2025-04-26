using Ambev.DeveloperEvaluation.Domain.Enums; // For mapping SaleStatus
using System;
using System.Collections.Generic;

namespace Ambev.DeveloperEvaluation.Application.Sales.DTOs;

/// <summary>
/// DTO representing a Sale, including its items and related information.
/// </summary>
public record SaleDto
{
    public Guid Id { get; init; }
    public string SaleNumber { get; init; }
    public DateTime SaleDate { get; init; }
    public CustomerInfoDto Customer { get; init; }
    public BranchInfoDto Branch { get; init; }
    public string Status { get; init; } // String representation of SaleStatus enum
    public decimal TotalSaleAmount { get; init; }
    public List<SaleItemDto> Items { get; init; } = new();
}
