using Ambev.DeveloperEvaluation.Application.Sales.DTOs;
using Ambev.DeveloperEvaluation.Common.Results;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Ambev.DeveloperEvaluation.Application.Sales.Queries
{
    /// <summary>
    /// Query to retrieve a paginated list of sales.
    /// Filtering and sorting parameters can be added later.
    /// </summary>
    public record ListSalesQuery : IRequest<PagedResult<SaleDto>>
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10; // Default page size
        private int _pageNumber = 1; // Default page number

        [Range(1, int.MaxValue)]
        public int PageNumber
        {
            get => _pageNumber;
            init => _pageNumber = value;
        }

        [Range(1, MaxPageSize)]
        public int PageSize
        {
            get => _pageSize;
            init => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        // Add properties for filtering (e.g., CustomerId, StartDate, EndDate, Status) and sorting later.
        // public string? CustomerId { get; init; }
        // public DateTime? StartDate { get; init; }
        // public DateTime? EndDate { get; init; }
        // public string? Status { get; init; }
        // public string? SortBy { get; init; } // e.g., "SaleDate", "TotalAmount"
        // public string? SortDirection { get; init; } // e.g., "ASC", "DESC"
    }
}
