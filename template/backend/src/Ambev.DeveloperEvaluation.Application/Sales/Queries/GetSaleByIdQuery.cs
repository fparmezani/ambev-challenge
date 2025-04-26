using Ambev.DeveloperEvaluation.Application.Sales.DTOs;
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace Ambev.DeveloperEvaluation.Application.Sales.Queries
{
    /// <summary>
    /// Query to retrieve a specific sale by its ID.
    /// </summary>
    public record GetSaleByIdQuery : IRequest<SaleDto?> // Returns SaleDto or null if not found
    {
        [Required]
        public Guid SaleId { get; init; }

        public GetSaleByIdQuery(Guid saleId)
        {
            SaleId = saleId;
        }
    }
}
