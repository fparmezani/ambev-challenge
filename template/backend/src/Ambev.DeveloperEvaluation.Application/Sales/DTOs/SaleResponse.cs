using System;
using System.Collections.Generic;

namespace Ambev.DeveloperEvaluation.Application.Sales.DTOs;

/// <summary>
/// DTO for returning Sale details.
/// </summary>
public class SaleResponse
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; }
    public DateTime SaleDate { get; set; }
    public ExternalIdentityDto Customer { get; set; }
    public ExternalIdentityDto Branch { get; set; }
    public List<SaleItemDto> Items { get; set; } = new List<SaleItemDto>();
    public decimal TotalSaleAmount { get; set; }
    public bool IsCancelled { get; set; }
}
