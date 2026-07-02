using ReceiptsApp.Domain.Exceptions;

namespace ReceiptsApp.Domain.Entities;

/// <summary>A retail market/store chain a receipt can be associated with.</summary>
public class Market : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string Country { get; private set; } = default!;

    private readonly List<Receipt> _receipts = new();
    public IReadOnlyCollection<Receipt> Receipts => _receipts.AsReadOnly();

    private Market() { }

    public static Market Create(string name, string country)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Market name is required.");
        if (string.IsNullOrWhiteSpace(country))
            throw new DomainValidationException("Market country is required.");

        return new Market
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Country = country.Trim()
        };
    }
}
