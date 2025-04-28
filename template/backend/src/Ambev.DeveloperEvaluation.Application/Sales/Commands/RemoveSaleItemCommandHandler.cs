using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands
{
    public class RemoveSaleItemCommandHandler : IRequestHandler<RemoveSaleItemCommand, Unit>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly ILogger<RemoveSaleItemCommandHandler> _logger;

        public RemoveSaleItemCommandHandler(
            ISaleRepository saleRepository,
            ILogger<RemoveSaleItemCommandHandler> logger)
        {
            _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(RemoveSaleItemCommand request, CancellationToken cancellationToken)
        {
            var sale = await _saleRepository.GetByIdAsync(request.SaleId);

            if (sale == null)
            {
                throw new DomainNotFoundException($"Sale with ID '{request.SaleId}' not found.");
            }

            // Domain entity handles logic (e.g., checking if item exists, cancelled status)
            try
            {
                sale.RemoveItem(request.ProductId);
            }
            catch (InvalidOperationException ex)
            {
                // Log and rethrow domain validation errors
                _logger.LogWarning("Failed to remove item for SaleId {SaleId}, ProductId {ProductId}: {ErrorMessage}", request.SaleId, request.ProductId, ex.Message);
                throw; // Rethrow to be handled by middleware/controller
            }
            // Note: If RemoveItem doesn't throw when product not found, we might not need specific handling here.

            await _saleRepository.UpdateAsync(sale);

            // Log the 'ItemCancelled' or 'SaleModified' event
            _logger.LogInformation("Domain Event: ItemCancelled - SaleId: {SaleId}, ProductId: {ProductId}", sale.Id, request.ProductId);
            // Or potentially: _logger.LogInformation("Domain Event: SaleModified - SaleId: {SaleId}, Reason: Item removed: {ProductId}", sale.Id, request.ProductId);

            return Unit.Value;
        }
    }
}
