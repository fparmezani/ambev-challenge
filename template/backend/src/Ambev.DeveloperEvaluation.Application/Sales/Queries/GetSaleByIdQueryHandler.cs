using Ambev.DeveloperEvaluation.Application.Sales.DTOs;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Queries
{
    public class GetSaleByIdQueryHandler : IRequestHandler<GetSaleByIdQuery, SaleDto?>
    {
        private readonly ISaleRepository _saleRepository;

        // Optional: Inject AutoMapper or similar if mapping becomes complex
        public GetSaleByIdQueryHandler(ISaleRepository saleRepository)
        {
            _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
        }

        public async Task<SaleDto?> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
        {
            var sale = await _saleRepository.GetByIdAsync(request.SaleId);

            if (sale == null)
            {
                return null; // Or throw DomainNotFoundException if preferred behavior
            }

            // Manual mapping from Sale entity to SaleDto
            var saleDto = MapToDto(sale);

            return saleDto;
        }

        // Helper method for mapping (could be moved to a dedicated mapper class/extension)
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
                Status = sale.Status.ToString(), // Convert enum to string
                TotalSaleAmount = sale.TotalSaleAmount,
                Items = sale.Items.Select(item => new SaleItemDto
                {
                    Product = new ProductInfoDto // Map Product Info
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
