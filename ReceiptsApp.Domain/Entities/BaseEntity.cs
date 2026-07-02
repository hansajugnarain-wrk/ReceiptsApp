namespace ReceiptsApp.Domain.Entities;

/// <summary>
/// Base type for all aggregate roots and entities. Centralizes identity comparison
/// so individual entities don't reimplement Equals/GetHashCode.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
