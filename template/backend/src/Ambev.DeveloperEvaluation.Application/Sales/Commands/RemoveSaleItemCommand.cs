using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands
{
    /// <summary>
    /// Command to remove an item from a sale.
    /// </summary>
    public record RemoveSaleItemCommand : IRequest<Unit> // Or IRequest
    {
        [Required]
        public Guid SaleId { get; init; }

        [Required]
        public string ProductId { get; init; }
    }
}
