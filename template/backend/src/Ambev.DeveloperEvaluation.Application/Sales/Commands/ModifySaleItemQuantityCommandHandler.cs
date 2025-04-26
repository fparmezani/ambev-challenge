using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Ambev.DeveloperEvaluation.Domain.Exceptions; // Assuming a custom exception for Not Found

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands
{
    public class ModifySaleItemQuantityCommandHandler : IRequestHandler<ModifySaleItemQuantityCommand, Unit>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly ILogger<ModifySaleItemQuantityCommandHandler> _logger;

        public ModifySaleItemQuantityCommandHandler(
            ISaleRepository saleRepository,
            ILogger<ModifySaleItemQuantityCommandHandler> logger)
        {
            _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(ModifySaleItemQuantityCommand request, CancellationToken cancellationToken)
        {
            var sale = await _saleRepository.GetByIdAsync(request.SaleId);

            if (sale == null)
            {
                // Consider using a specific NotFoundException
                throw new DomainNotFoundException($"Sale with ID '{request.SaleId}' not found.");
            }

            // Domain entity handles the logic and validation (max quantity, cancelled status)
            try
            {
                sale.ModifyItemQuantity(request.ProductId, request.NewQuantity);
            }
            catch (Exception ex) when (ex is ArgumentOutOfRangeException || ex is InvalidOperationException || ex is KeyNotFoundException)
            {
                // Log and rethrow domain validation errors as potential bad requests or specific errors
                _logger.LogWarning("Failed to modify item quantity for SaleId {SaleId}, ProductId {ProductId}: {ErrorMessage}", request.SaleId, request.ProductId, ex.Message);
                throw; // Rethrow to be handled by middleware/controller
            }

            await _saleRepository.UpdateAsync(sale);

            // Log the 'SaleModified' event
            _logger.LogInformation("Domain Event: SaleModified - SaleId: {SaleId}, Reason: Item quantity modified for ProductId: {ProductId}", sale.Id, request.ProductId);

            return Unit.Value;
        }
    }
}
