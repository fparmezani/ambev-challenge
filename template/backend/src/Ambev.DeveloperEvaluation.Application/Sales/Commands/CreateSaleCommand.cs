using Ambev.DeveloperEvaluation.Application.Sales.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands
{
    /// <summary>
    /// Command to create a new sale.
    /// </summary>
    public record CreateSaleCommand : IRequest<Guid>
    {
        [Required]
        public string SaleNumber { get; init; }

        [Required]
        public CustomerInfoDto Customer { get; init; }

        [Required]
        public BranchInfoDto Branch { get; init; }

        [Required]
        [MinLength(1)]
        public List<CreateSaleItemDto> Items { get; init; } = new();

        /// <summary>
        /// Represents an item to be included in the new sale.
        /// </summary>
        public record CreateSaleItemDto
        {
            [Required]
            public string ProductId { get; init; }

            // Product Name/Description can be looked up via ProductId if needed,
            // or passed directly if the source system provides them.
            // Assuming ProductId is sufficient to fetch details.

            [Required]
            [Range(1, 20)]
            public int Quantity { get; init; }

            [Required]
            [Range(0.01, (double)decimal.MaxValue)]
            public decimal UnitPrice { get; init; }
        }
    }
}
