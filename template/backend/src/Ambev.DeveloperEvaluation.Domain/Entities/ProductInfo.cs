namespace Ambev.DeveloperEvaluation.Domain.Entities
{
    /// <summary>
    /// Represents product information as an Owned Entity/Value Object. Use record class for reference type semantics required by EF Core Owned Types.
    /// </summary>
    public record class ProductInfo(string ProductId, string Name, string Description);
}
