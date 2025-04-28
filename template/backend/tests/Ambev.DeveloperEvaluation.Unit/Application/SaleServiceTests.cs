using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.DTOs;
using Ambev.DeveloperEvaluation.Application.Sales.Services;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class SaleServiceTests
{
    private readonly ISaleRepository _mockSaleRepository;
    private readonly ILogger<SaleService> _mockLogger;
    private readonly SaleService _saleService;

    public SaleServiceTests()
    {
        _mockSaleRepository = Substitute.For<ISaleRepository>();
        _mockLogger = Substitute.For<ILogger<SaleService>>();
        _saleService = new SaleService(_mockSaleRepository, _mockLogger);
    }

    // --- Helpers ---
    private ExternalIdentityDto CreateDummyCustomerDto() => new() { Id = "C1", Description = "Cust" };
    private ExternalIdentityDto CreateDummyBranchDto() => new() { Id = "B1", Description = "Branch" };
    private ProductInfoDto CreateDummyProductDto(string id = "P1", string desc = "Prod") => new() { ProductId = id, Description = desc };

    private CreateSaleRequest CreateValidSaleRequest(string saleNumber = "S1") => new CreateSaleRequest
    {
        SaleNumber = saleNumber,
        Customer = CreateDummyCustomerDto(),
        Branch = CreateDummyBranchDto(),
        Items = new List<SaleItemDto>
        {
            new() { Product = CreateDummyProductDto("P1"), Quantity = 5, UnitPrice = 10.0m }, // 10% discount
            new() { Product = CreateDummyProductDto("P2"), Quantity = 2, UnitPrice = 20.0m }  // 0% discount
        }
    };

    private Sale CreateFakeSaleFromRequest(CreateSaleRequest request, Guid? id = null)
    {
        var customer = new CustomerInfo(request.Customer.Id, request.Customer.Description);
        var branch = new BranchInfo(request.Branch.Id, request.Branch.Description);
        var items = request.Items.Select(dto => new SaleItem(new ProductInfo(dto.Product.ProductId, dto.Product.Description,""), dto.Quantity, dto.UnitPrice)).ToList();
        var sale = new Sale(request.SaleNumber, customer, branch, items);
        // Allow overriding ID for testing GetById etc.
        if(id.HasValue) { typeof(Sale).GetProperty(nameof(Sale.Id))?.SetValue(sale, id.Value); }
        return sale;
    }

    // --- Tests ---

    [Fact]
    public async Task CreateSaleAsync_WithValidRequest_ShouldCallRepoAddAndReturnResponse()
    {
        var request = CreateValidSaleRequest();
        var response = await _saleService.CreateSaleAsync(request);
        await _mockSaleRepository.Received(1).AddAsync(Arg.Any<Sale>());
        response.Should().NotBeNull();
        response.SaleNumber.Should().Be(request.SaleNumber);
        response.Items.Should().HaveCount(2);
        response.TotalSaleAmount.Should().Be(((5 * 10.0m) * 0.9m) + (2 * 20.0m));
        _mockLogger.ReceivedWithAnyArgs(1).LogInformation(default);
    }

    [Fact]
    public async Task CreateSaleAsync_WithItemQuantityAboveLimit_ShouldThrowAndLogWarning()
    {
        var request = CreateValidSaleRequest();
        request.Items.Add(new SaleItemDto { Product = CreateDummyProductDto("P3"), Quantity = 21, UnitPrice = 5m });
        Func<Task> action = async () => await _saleService.CreateSaleAsync(request);
        await action.Should().ThrowAsync<ArgumentOutOfRangeException>();
        await _mockSaleRepository.DidNotReceive().AddAsync(Arg.Any<Sale>());
        _mockLogger.ReceivedWithAnyArgs(1).LogWarning(default);
    }

    [Fact]
    public async Task GetSaleByIdAsync_WhenFound_ShouldReturnResponse()
    {
        var saleId = Guid.NewGuid();
        var fakeSale = CreateFakeSaleFromRequest(CreateValidSaleRequest(), saleId);
        _mockSaleRepository.GetByIdAsync(saleId).Returns(Task.FromResult<Sale?>(fakeSale));
        var response = await _saleService.GetSaleByIdAsync(saleId);
        response.Should().NotBeNull();
        response.Id.Should().Be(saleId);
    }

    [Fact]
    public async Task GetSaleByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        var saleId = Guid.NewGuid();
        _mockSaleRepository.GetByIdAsync(saleId).Returns(Task.FromResult<Sale?>(null));
        var response = await _saleService.GetSaleByIdAsync(saleId);
        response.Should().BeNull();
    }

    [Fact]
    public async Task CancelSaleAsync_WhenFoundAndNotCancelled_ShouldUpdateAndReturnTrue()
    {
        var saleId = Guid.NewGuid();
        var fakeSale = CreateFakeSaleFromRequest(CreateValidSaleRequest(), saleId);
        _mockSaleRepository.GetByIdAsync(saleId).Returns(Task.FromResult<Sale?>(fakeSale));
        var result = await _saleService.CancelSaleAsync(saleId);
        result.Should().BeTrue();
        await _mockSaleRepository.Received(1).UpdateAsync(Arg.Is<Sale>(s => s.Id == saleId && s.Status == DeveloperEvaluation.Domain.Enums.SaleStatus.Cancelled));
        _mockLogger.ReceivedWithAnyArgs(1).LogInformation(default);
    }

     [Fact]
    public async Task CancelSaleAsync_WhenNotFound_ShouldReturnFalse()
    {
        var saleId = Guid.NewGuid();
        _mockSaleRepository.GetByIdAsync(saleId).Returns(Task.FromResult<Sale?>(null));
        var result = await _saleService.CancelSaleAsync(saleId);
        result.Should().BeFalse();
        await _mockSaleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
    }

    [Fact]
    public async Task CancelSaleAsync_WhenAlreadyCancelled_ShouldReturnTrueWithoutUpdate()
    {
        var saleId = Guid.NewGuid();
        var fakeSale = CreateFakeSaleFromRequest(CreateValidSaleRequest(), saleId);
        fakeSale.CancelSale();
        _mockSaleRepository.GetByIdAsync(saleId).Returns(Task.FromResult<Sale?>(fakeSale));
        var result = await _saleService.CancelSaleAsync(saleId);
        result.Should().BeFalse();
        await _mockSaleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
    }

    
    [Fact]
    public async Task UpdateSaleAsync_WhenNotFound_ShouldReturnNull()
    {
        var saleId = Guid.NewGuid();
        _mockSaleRepository.GetByIdAsync(saleId).Returns(Task.FromResult<Sale?>(null));
        var updateRequest = CreateValidSaleRequest("S-NEW");
        var response = await _saleService.UpdateSaleAsync(saleId, updateRequest);
        response.Should().BeNull();
        await _mockSaleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
    }

    [Fact]
    public async Task UpdateSaleAsync_OnCancelledSale_ShouldThrowAndLogWarning()
    {
        var saleId = Guid.NewGuid();
        var existingSale = CreateFakeSaleFromRequest(CreateValidSaleRequest("S-OLD"), saleId);
        existingSale.CancelSale();
        _mockSaleRepository.GetByIdAsync(saleId).Returns(Task.FromResult<Sale?>(existingSale));
        var updateRequest = CreateValidSaleRequest("S-NEW");
        Func<Task> action = async () => await _saleService.UpdateSaleAsync(saleId, updateRequest);
        await action.Should().ThrowAsync<InvalidOperationException>();
        await _mockSaleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
        _mockLogger.ReceivedWithAnyArgs(1).LogWarning(default);
    }

    [Fact]
    public async Task UpdateSaleAsync_WithItemQuantityAboveLimit_ShouldThrowAndLogWarning()
    {
        var saleId = Guid.NewGuid();
        var existingSale = CreateFakeSaleFromRequest(CreateValidSaleRequest("S-OLD"), saleId);
       _mockSaleRepository.GetByIdAsync(saleId).Returns(Task.FromResult<Sale?>(existingSale));
        var updateRequest = CreateValidSaleRequest("S-NEW");
        updateRequest.Items.Add(new SaleItemDto { Product = CreateDummyProductDto("P3"), Quantity = 21, UnitPrice = 5m });

        Func<Task> action = async () => await _saleService.UpdateSaleAsync(saleId, updateRequest);
        await action.Should().ThrowAsync<ArgumentOutOfRangeException>();
        await _mockSaleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
        _mockLogger.ReceivedWithAnyArgs(1).LogWarning(default);
    }
}
