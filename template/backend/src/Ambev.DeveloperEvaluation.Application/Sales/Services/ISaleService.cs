using Ambev.DeveloperEvaluation.Application.Sales.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Application.Sales.Services;

/// <summary>
/// Interface for managing Sale application logic.
/// </summary>
public interface ISaleService
{
    Task<SaleResponse?> CreateSaleAsync(CreateSaleRequest request);
    Task<SaleResponse?> GetSaleByIdAsync(Guid id);
    Task<IEnumerable<SaleResponse>> GetAllSalesAsync();
    // Note: Update might need a different request DTO if you want more granular control
    // than just resubmitting the whole sale structure. For simplicity, reusing CreateSaleRequest.
    Task<SaleResponse?> UpdateSaleAsync(Guid id, CreateSaleRequest request);
    Task<bool> CancelSaleAsync(Guid id);
    // Future: Add methods for item modification/cancellation if required by API
    // Task<bool> CancelSaleItemAsync(Guid saleId, string productId);
    // Task<SaleResponse?> UpdateSaleItemQuantityAsync(Guid saleId, string productId, int newQuantity);
}
