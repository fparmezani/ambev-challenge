using Ambev.DeveloperEvaluation.Common.Results;
using Ambev.DeveloperEvaluation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks; // Using Tasks for async operations

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Interface defining repository operations for the Sale aggregate root.
/// </summary>
public interface ISaleRepository
{
    Task<Sale?> GetByIdAsync(Guid id);
    Task<IEnumerable<Sale>> GetAllAsync(); // Consider pagination for large datasets
    Task AddAsync(Sale sale);
    Task UpdateAsync(Sale sale);
    Task<PagedResult<Sale>> ListAsync(int pageNumber, int pageSize);
    
}
