namespace Ambev.DeveloperEvaluation.Domain.Common;

/// <summary>
/// Represents an identity reference to an entity in another domain/bounded context.
/// </summary>
/// <param name="Id">The unique identifier of the external entity.</param>
/// <param name="Description">A denormalized description or name of the external entity.</param>
public record ExternalIdentity(string Id, string Description);
