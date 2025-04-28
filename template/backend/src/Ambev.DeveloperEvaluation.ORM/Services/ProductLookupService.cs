using Ambev.DeveloperEvaluation.Application.Sales.Commands;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.ORM.Services
{
    /// <summary>
    /// Implementation of IProductLookupService that retrieves product information from the database
    /// </summary>
    public class ProductLookupService : IProductLookupService
    {
        private readonly DefaultContext _dbContext;
        private readonly ILogger<ProductLookupService> _logger;

        public ProductLookupService(DefaultContext dbContext, ILogger<ProductLookupService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves product information by ID asynchronously
        /// </summary>
        /// <param name="productId">The ID of the product to retrieve</param>
        /// <returns>ProductInfo object if found, null otherwise</returns>
        public async Task<ProductInfo?> GetProductInfoByIdAsync(string productId)
        {
            _logger.LogInformation("Looking up product with ID: {ProductId}", productId);

            
            if (string.IsNullOrEmpty(productId))
            {
                _logger.LogWarning("Product ID was null or empty");
                return null;
            }

            // Simulate a few known products
            return productId switch
            {
                "PROD-001" => new ProductInfo("PROD-001", "Beer 350ml", "Regular beer can 350ml"),
                "PROD-002" => new ProductInfo("PROD-002", "Beer 600ml", "Beer bottle 600ml"),
                "PROD-003" => new ProductInfo("PROD-003", "Beer 1L", "Beer bottle 1 liter"),
                _ => null
            };
        }
    }
}
