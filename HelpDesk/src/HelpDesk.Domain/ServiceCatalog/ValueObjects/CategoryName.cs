using HelpDesk.Domain.SharedKernel.Exceptions;
using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.ServiceCatalog.ValueObjects;

public sealed class CategoryName : ValueObject
{
    public const int MaxLength = 180;
    public string Value { get; }

    private CategoryName(string value) => Value = value;

    public static CategoryName Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new DomainException("Nome é obrigatório.");

        var name = raw.Trim();

        if (name.Length > MaxLength)
            throw new DomainException($"O nome da categoria excede o limite de {MaxLength} caracteres (atual: {name.Length}).");

        return new CategoryName(name);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
