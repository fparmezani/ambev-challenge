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
    // Delete is often handled by marking as cancelled or soft delete rather than physical removal.
    // If hard delete is required: Task DeleteAsync(Guid id);
    // Consider adding methods for specific queries if needed, e.g., GetBySaleNumberAsync
}
