using Ambev.DeveloperEvaluation.Application.Sales.Commands;
using Ambev.DeveloperEvaluation.Application.Sales.Queries;
using Ambev.DeveloperEvaluation.Application.Sales.DTOs; // For SaleDto
using Ambev.DeveloperEvaluation.Common.Results; // For PagedResult
using Ambev.DeveloperEvaluation.Domain.Exceptions; // For DomainNotFoundException
using Ambev.DeveloperEvaluation.WebApi.Common; // For BaseController
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations; // For ValidationException
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.WebApi.Controllers
{
    public class SalesController : BaseController // Inherit from BaseController
    {
        private readonly IMediator _mediator;

        public SalesController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        // POST api/sales
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseWithData<Guid>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        public async Task<IActionResult> CreateSale([FromBody] CreateSaleCommand command)
        {
            try
            {
                var saleId = await _mediator.Send(command);
                // Use the Created helper from BaseController
                return Created(nameof(GetSaleById), new { id = saleId }, saleId);
            }
            catch (ValidationException ex) // Catch validation errors from command
            {
                return BadRequest(ex.ValidationResult.ErrorMessage); // Or format multiple errors
            }
            catch (KeyNotFoundException ex) // Catch errors finding product
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentOutOfRangeException ex) // Catch quantity/price errors
            {
                 return BadRequest(ex.Message);
            }
        }

        // GET api/sales/{id}
        [HttpGet("{id}", Name = nameof(GetSaleById))]
        [ProducesResponseType(typeof(ApiResponseWithData<SaleDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        public async Task<IActionResult> GetSaleById(Guid id)
        {
            var query = new GetSaleByIdQuery(id);
            var result = await _mediator.Send(query);

            return result != null ? Ok(result) : NotFound();
        }

        // GET api/sales
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<SaleDto>), 200)] // Use PaginatedResponse from BaseController
        public async Task<IActionResult> ListSales([FromQuery] ListSalesQuery query)
        {
            // MediatR handler returns PagedResult<SaleDto>
            var result = await _mediator.Send(query ?? new ListSalesQuery());

            // We need to map PagedResult<SaleDto> to PaginatedList<SaleDto> expected by BaseController helper
            // Or adjust BaseController/Create a new helper
            // Quick fix: Create PaginatedList manually (Assuming PagedResult has needed props)
            var paginatedList = new Ambev.DeveloperEvaluation.WebApi.Common.PaginatedList<SaleDto>(result.Items.ToList(), result.TotalCount, result.PageNumber, result.PageSize);

            return OkPaginated<SaleDto>(paginatedList); // Use the helper, specifying type explicitly
        }

        // PATCH api/sales/{id}/items/{productId}
        [HttpPatch("{id}/items/{productId}")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        public async Task<IActionResult> ModifyItemQuantity(Guid id, string productId, [FromBody] ModifyQuantityRequest request)
        {
            var command = new ModifySaleItemQuantityCommand
            {
                SaleId = id,
                ProductId = productId,
                NewQuantity = request.NewQuantity
            };

            try
            {
                await _mediator.Send(command);
                return Ok(); // Simple Ok response
            }
            catch (DomainNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex) when (ex is ArgumentOutOfRangeException || ex is InvalidOperationException || ex is KeyNotFoundException)
            {
                return BadRequest(ex.Message); // Domain rule violations
            }
        }

        // DELETE api/sales/{id}/items/{productId}
        [HttpDelete("{id}/items/{productId}")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 400)] // For invalid ops like deleting from cancelled sale
        public async Task<IActionResult> RemoveItem(Guid id, string productId)
        {
            var command = new RemoveSaleItemCommand { SaleId = id, ProductId = productId };
            try
            {
                await _mediator.Send(command);
                return Ok();
            }
            catch (DomainNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
             catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message); // e.g., Sale is cancelled
            }
        }

        // POST api/sales/{id}/cancel
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 400)] // For invalid ops like cancelling already cancelled sale
        public async Task<IActionResult> CancelSale(Guid id)
        {
            var command = new CancelSaleCommand { SaleId = id };
            try
            {
                await _mediator.Send(command);
                return Ok();
            }
            catch (DomainNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message); // e.g., Sale already cancelled
            }
        }

        // Simple DTO for the body of ModifyItemQuantity
        public record ModifyQuantityRequest([Required][Range(1, 20)] int NewQuantity);
    }

    // Helper PaginatedList class used by BaseController (ensure it exists or define it)
    // If it doesn't exist in WebApi.Common, we should add it.
    // Assuming it looks something like this for now:
    public class PaginatedList<T>
    {
        public List<T> Items { get; }
        public int CurrentPage { get; }
        public int TotalPages { get; }
        public int PageSize { get; }
        public int TotalCount { get; }

        public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            Items = items;
        }

         public bool HasPreviousPage => CurrentPage > 1;
         public bool HasNextPage => CurrentPage < TotalPages;

         // Static creation method might be useful if needed elsewhere
         // public static PaginatedList<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
         // { ... implementation ... }
    }
}
