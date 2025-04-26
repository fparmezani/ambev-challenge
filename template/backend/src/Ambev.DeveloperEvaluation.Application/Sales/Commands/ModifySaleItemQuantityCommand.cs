using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands
{
    /// <summary>
    /// Command to modify the quantity of an item within a sale.
    /// </summary>
    public record ModifySaleItemQuantityCommand : IRequest<Unit> // Or IRequest if you prefer explicit void
    {
        [Required]
        public Guid SaleId { get; init; }

        [Required]
        public string ProductId { get; init; }

        [Required]
        [Range(1, 20)] // Enforce quantity rules
        public int NewQuantity { get; init; }
    }
}
