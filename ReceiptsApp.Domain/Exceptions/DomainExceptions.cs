namespace ReceiptsApp.Domain.Exceptions;

/// <summary>
/// Thrown when an entity is constructed or mutated in a way that violates a business invariant.
/// Reserved for programmer errors and truly invalid domain state — not for expected
/// "user typed the wrong password" outcomes, which the Application layer models with Result&lt;T&gt;.
/// </summary>
public class DomainValidationException : Exception
{
    public DomainValidationException(string message) : base(message) { }
}

/// <summary>Thrown when domain code expects an entity to exist and it does not.</summary>
public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.") { }
}
