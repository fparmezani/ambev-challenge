using System;

namespace Ambev.DeveloperEvaluation.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested domain entity is not found.
    /// </summary>
    public class DomainNotFoundException : Exception
    {
        public DomainNotFoundException() { }

        public DomainNotFoundException(string message) : base(message) { }

        public DomainNotFoundException(string message, Exception innerException) : base(message, innerException) { }

        // Consider adding properties like EntityType and EntityId if needed for more context
        // public string EntityType { get; }
        // public object EntityId { get; }
    }
}
