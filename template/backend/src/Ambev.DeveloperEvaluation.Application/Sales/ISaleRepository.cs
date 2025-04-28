using Ambev.DeveloperEvaluation.Common.Results;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales
{
    /// <summary>
    /// Defines the contract for accessing Sale data.
    /// </summary>
    public interface ISaleRepository1
    {
        /// <summary>
        /// Gets a sale by its unique identifier.
        /// </summary>
        /// <param name="id">The sale ID.</param>
        /// <returns>The sale entity or null if not found.</returns>
        Task<Sale?> GetByIdAsync(Guid id);

        /// <summary>
        /// Adds a new sale to the repository.
        /// </summary>
        /// <param name="sale">The sale entity to add.</param>
        Task AddAsync(Sale sale);

        /// <summary>
        /// Updates an existing sale in the repository.
        /// </summary>
        /// <param name="sale">The sale entity with updated information.</param>
        Task UpdateAsync(Sale sale);

        // Add method for listing sales with pagination
        // Filtering/Sorting parameters can be added later via a criteria object or separate parameters
        Task<PagedResult<Sale>> ListAsync(int pageNumber, int pageSize);
    }
}
