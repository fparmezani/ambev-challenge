using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands
{
    public interface IProductLookupService
    {
        Task<ProductInfo?> GetProductInfoByIdAsync(string productId);
    }

    public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, Guid>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IProductLookupService _productLookupService; // Assumed interface
        private readonly ILogger<CreateSaleCommandHandler> _logger;

        public CreateSaleCommandHandler(
            ISaleRepository saleRepository,
            IProductLookupService productLookupService, // Inject the lookup service
            ILogger<CreateSaleCommandHandler> logger)
        {
            _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
            _productLookupService = productLookupService ?? throw new ArgumentNullException(nameof(productLookupService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
        {
            // 1. Map Customer and Branch Info from DTOs (Direct mapping as they are simple records)
            var customerInfo = new CustomerInfo(request.Customer.CustomerId, request.Customer.Name);
            var branchInfo = new BranchInfo(request.Branch.BranchId, request.Branch.Name);

            // 2. Fetch ProductInfo for each item and create SaleItem entities
            var saleItems = new List<SaleItem>();
            foreach (var itemDto in request.Items)
            {
                var productInfo = await _productLookupService.GetProductInfoByIdAsync(itemDto.ProductId);
                if (productInfo == null)
                {
                    // Handle case where product ID is invalid
                    throw new KeyNotFoundException($"Product with ID '{itemDto.ProductId}' not found.");
                }

                // Create SaleItem using fetched ProductInfo and DTO data
                // The SaleItem constructor applies discount rules and validates quantity
                // Use null-forgiving operator (!) because the null check above guarantees non-null.
                saleItems.Add(new SaleItem(productInfo!, itemDto.Quantity, itemDto.UnitPrice));
            }

            // 3. Create the Sale aggregate
            // The Sale constructor aggregates items and ensures rules (like max quantity check if items are combined)
            var sale = new Sale(request.SaleNumber, customerInfo, branchInfo, saleItems);

            // 4. Persist the Sale
            await _saleRepository.AddAsync(sale);

            // 5. Log the 'SaleCreated' event (as per optional requirement)
            _logger.LogInformation("Domain Event: SaleCreated - SaleId: {SaleId}, SaleNumber: {SaleNumber}", sale.Id, sale.SaleNumber);

            // 6. Return the new Sale's ID
            return sale.Id;
        }
    }
}
