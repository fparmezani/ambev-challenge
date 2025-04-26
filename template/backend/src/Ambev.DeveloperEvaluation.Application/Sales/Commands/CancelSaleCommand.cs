using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands
{
    /// <summary>
    /// Command to cancel an existing sale.
    /// </summary>
    public record CancelSaleCommand : IRequest<Unit> // Or IRequest
    {
        [Required]
        public Guid SaleId { get; init; }
    }
}
