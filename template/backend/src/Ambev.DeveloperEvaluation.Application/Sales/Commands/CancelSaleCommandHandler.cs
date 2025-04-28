using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands
{
    public class CancelSaleCommandHandler : IRequestHandler<CancelSaleCommand, Unit>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly ILogger<CancelSaleCommandHandler> _logger;

        public CancelSaleCommandHandler(
            ISaleRepository saleRepository,
            ILogger<CancelSaleCommandHandler> logger)
        {
            _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
        {
            var sale = await _saleRepository.GetByIdAsync(request.SaleId);

            if (sale == null)
            {
                throw new DomainNotFoundException($"Sale with ID '{request.SaleId}' not found.");
            }

            // Domain entity handles logic (e.g., checking current status)
            try
            {
                sale.CancelSale();
            }
            catch (InvalidOperationException ex)
            {
                // Log and rethrow domain validation errors (e.g., already cancelled)
                _logger.LogWarning("Failed to cancel SaleId {SaleId}: {ErrorMessage}", request.SaleId, ex.Message);
                throw; // Rethrow to be handled by middleware/controller
            }

            await _saleRepository.UpdateAsync(sale);

            // Log the 'SaleCancelled' event
            _logger.LogInformation("Domain Event: SaleCancelled - SaleId: {SaleId}", sale.Id);

            return Unit.Value;
        }
    }
}
