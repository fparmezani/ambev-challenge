using Ambev.DeveloperEvaluation.Application.Sales.DTOs;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Services;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly ILogger<SaleService> _logger;

    public SaleService(ISaleRepository saleRepository, ILogger<SaleService> logger)
    {
        _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // Inject logger
    }

    public async Task<SaleResponse?> CreateSaleAsync(CreateSaleRequest request)
    {
        try
        {
            // Validate command data (basic example, enhance as needed)
            if (request.Customer == null || request.Branch == null || request.Items == null || !request.Items.Any())
            {
                throw new ArgumentException("Invalid sale creation data.");
            }

            var customerInfo = new CustomerInfo(request.Customer.Id, request.Customer.Description);
            var branchInfo = new BranchInfo(request.Branch.Id, request.Branch.Description);

            var saleItems = request.Items.Select(itemDto => {
                 // Additional check here for max quantity rule, although domain entity also checks
                 if (itemDto.Quantity > 20)
                 {
                      throw new ArgumentOutOfRangeException(nameof(itemDto.Quantity), $"Quantity for product '{itemDto.Product.Description}' cannot exceed 20.");
                 }
                 var productIdentity = new ExternalIdentity(itemDto.Product.ProductId, itemDto.Product.Description);
                 // Domain SaleItem constructor calculates discount based on quantity
                 var productInfo = new ProductInfo(productIdentity.Id, productIdentity.Description, "");
                 return new SaleItem(productInfo, itemDto.Quantity, itemDto.UnitPrice);
             }).ToList(); // Evaluate LINQ query

            // Domain Sale constructor enforces rules like non-empty sale number
            var sale = new Sale(request.SaleNumber, customerInfo, branchInfo, saleItems);

            await _saleRepository.AddAsync(sale);

            _logger.LogInformation("SaleCreated: Id={SaleId}, Number={SaleNumber}, Total={TotalAmount}", sale.Id, sale.SaleNumber, sale.TotalSaleAmount);

            return MapSaleToResponseDto(sale);
        }
        catch (ArgumentException ex) // Catch validation errors from domain/DTO
        {
            _logger.LogWarning("Validation error creating sale: {ErrorMessage}", ex.Message);
            throw; // Re-throw to be handled by global error handler or controller
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sale for SaleNumber {SaleNumber}", request.SaleNumber);
            return null; // Or throw a custom application exception
        }
    }

    public async Task<SaleResponse?> GetSaleByIdAsync(Guid id)
    {
        try
        {
            var sale = await _saleRepository.GetByIdAsync(id);
            return sale == null ? null : MapSaleToResponseDto(sale);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sale with Id {SaleId}", id);
            return null;
        }
    }

    public async Task<IEnumerable<SaleResponse>> GetAllSalesAsync()
    {
        try
        {
            // TODO: Implement proper fetching of *all* sales if needed. This fetches only the first 1000.
            var pagedResult = await _saleRepository.ListAsync(1, 1000); // Fetch page 1, size 1000
            var sales = pagedResult.Items;
            return sales.Select(MapSaleToResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all sales");
            return Enumerable.Empty<SaleResponse>(); // Return empty list on failure
        }
    }

    // Note: Update logic needs refinement based on exact requirements (e.g., how to handle item updates).
    // This version replaces the entire item list, which might not be intended.
    public async Task<SaleResponse?> UpdateSaleAsync(Guid id, CreateSaleRequest request)
    {
       try
        {
            var existingSale = await _saleRepository.GetByIdAsync(id);
            if (existingSale == null) return null; // Not found

            if (existingSale.Status == SaleStatus.Cancelled)
            {
                throw new InvalidOperationException("Cannot modify a cancelled sale.");
            }

            // Map updated details - Consider adding specific update methods to the Sale entity
            var customerInfo = new CustomerInfo(request.Customer.Id, request.Customer.Description);
            var branchInfo = new BranchInfo(request.Branch.Id, request.Branch.Description);

            // Simplistic update: Clear existing items and add new ones.
            // A better approach: Identify changes and use domain methods like ModifyItemQuantity, AddItem, RemoveItem.
            while(existingSale.Items.Any()) {
                existingSale.RemoveItem(existingSale.Items.First().Product.ProductId); // Assumes RemoveItem exists and works
            }

             foreach (var itemDto in request.Items)
             {
                 // Apply validation rule check again for update
                 if (itemDto.Quantity > 20)
                 {
                     throw new ArgumentOutOfRangeException(nameof(itemDto.Quantity), $"Quantity for product '{itemDto.Product.Description}' cannot exceed 20.");
                 }
                 var productIdentity = new ExternalIdentity(itemDto.Product.ProductId, itemDto.Product.Description);
                 // AddItem in Sale entity should handle discount calculation and rules
                 var productInfo = new ProductInfo(productIdentity.Id, productIdentity.Description, "");
                 existingSale.AddItem(productInfo, itemDto.Quantity, itemDto.UnitPrice);
             }
             // TODO: Add domain methods to update SaleNumber, Customer, Branch if needed, e.g., existingSale.UpdateDetails(...)

            await _saleRepository.UpdateAsync(existingSale);

             _logger.LogInformation("SaleModified: Id={SaleId}", existingSale.Id);

            return MapSaleToResponseDto(existingSale);
        }
        catch (ArgumentException ex) // Catch validation errors
        {
            _logger.LogWarning("Validation error updating sale {SaleId}: {ErrorMessage}", id, ex.Message);
            throw;
        }
       catch (InvalidOperationException ex) // Catch business rule violation
        {
             _logger.LogWarning("Invalid operation updating sale {SaleId}: {ErrorMessage}", id, ex.Message);
             throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sale with Id {SaleId}", id);
            return null;
        }
    }

    public async Task<bool> CancelSaleAsync(Guid id)
    {
        try
        {
            var sale = await _saleRepository.GetByIdAsync(id);
            if (sale == null) return false; // Not found

            if (sale.Status == SaleStatus.Cancelled)
            {
                throw new InvalidOperationException("Sale is already cancelled.");
            }

            sale.CancelSale(); // Use domain method

            await _saleRepository.UpdateAsync(sale);

             _logger.LogInformation("SaleCancelled: Id={SaleId}", sale.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling sale with Id {SaleId}", id);
            return false;
        }
    }

    // Helper method for mapping Sale entity to SaleResponse DTO
    private SaleResponse MapSaleToResponseDto(Sale sale)
    {
        return new SaleResponse
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            SaleDate = sale.SaleDate,
            Customer = new ExternalIdentityDto { Id = sale.Customer.CustomerId, Description = sale.Customer.Name },
            Branch = new ExternalIdentityDto { Id = sale.Branch.BranchId, Description = sale.Branch.Name },
            Items = sale.Items.Select(item => new SaleItemDto
            {
                Product = new ProductInfoDto { ProductId = item.Product.ProductId, Name = item.Product.Name, Description = item.Product.Description },
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Discount = item.Discount, // Calculated by domain
                TotalItemAmount = item.TotalItemAmount // Calculated by domain
            }).ToList(),
            TotalSaleAmount = sale.TotalSaleAmount,
            IsCancelled = sale.Status == SaleStatus.Cancelled
        };
    }
}
