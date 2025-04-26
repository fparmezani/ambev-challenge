using System;
using System.Collections.Generic;

namespace Ambev.DeveloperEvaluation.Common.Results
{
    /// <summary>
    /// Represents a paginated list of items.
    /// </summary>
    /// <typeparam name="T">The type of the items in the list.</typeparam>
    public record PagedResult<T>
    {
        public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        // Constructor for convenience
        public PagedResult(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount)
        {
            Items = items;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        // Empty result constructor
        public PagedResult() { }

        public static PagedResult<T> Empty(int pageNumber, int pageSize) => new PagedResult<T>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = 0,
            Items = Enumerable.Empty<T>()
        };
    }
}
