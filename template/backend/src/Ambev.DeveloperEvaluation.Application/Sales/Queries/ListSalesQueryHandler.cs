using Ambev.DeveloperEvaluation.Application.Sales.DTOs;
using Ambev.DeveloperEvaluation.Common.Results;
using Ambev.DeveloperEvaluation.Domain.Entities;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Application.Sales.Queries
{
    public class ListSalesQueryHandler : IRequestHandler<ListSalesQuery, PagedResult<SaleDto>>
    {
        private readonly ISaleRepository _saleRepository;

        // Optional: Inject AutoMapper or similar if mapping becomes complex
        public ListSalesQueryHandler(ISaleRepository saleRepository)
        {
            _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
        }

        public async Task<PagedResult<SaleDto>> Handle(ListSalesQuery request, CancellationToken cancellationToken)
        {
            // Retrieve the paginated list of Sale entities from the repository
            // The repository implementation will handle the actual data fetching and pagination logic.
            var pagedSales = await _saleRepository.ListAsync(request.PageNumber, request.PageSize);

            // Map the Sale entities to SaleDto objects
            var saleDtos = pagedSales.Items.Select(MapToDto).ToList();

            // Create the PagedResult for DTOs
            var pagedResultDto = new PagedResult<SaleDto>(
                items: saleDtos,
                pageNumber: pagedSales.PageNumber,
                pageSize: pagedSales.PageSize,
                totalCount: pagedSales.TotalCount
            );

            return pagedResultDto;
        }

        // Re-use the same mapping logic as GetSaleByIdQueryHandler
        // Consider moving this to a shared location (e.g., a static Mapper class or Extension Methods)
        private static SaleDto MapToDto(Sale sale)
        {
             return new SaleDto
            {
                Id = sale.Id,
                SaleNumber = sale.SaleNumber,
                SaleDate = sale.SaleDate,
                Customer = new CustomerInfoDto
                {
                    CustomerId = sale.Customer.CustomerId,
                    Name = sale.Customer.Name
                },
                Branch = new BranchInfoDto
                {
                    BranchId = sale.Branch.BranchId,
                    Name = sale.Branch.Name
                },
                Status = sale.Status.ToString(),
                TotalSaleAmount = sale.TotalSaleAmount,
                Items = sale.Items.Select(item => new SaleItemDto
                {
                    Product = new ProductInfoDto
                    {
                        ProductId = item.Product.ProductId,
                        Name = item.Product.Name,
                        Description = item.Product.Description
                    },
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Discount = item.Discount,
                    TotalItemAmount = item.TotalItemAmount
                }).ToList()
            };
        }
    }
}
