using HelpDesk.Domain.SharedKernel.Exceptions;
using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.Ticketing.ValueObjects
{
    public sealed class TicketTitle : ValueObject
    {
        public const int MaxLength = 180;
        public string Value { get; }

        private TicketTitle(string value) => Value = value;

        public static TicketTitle Create(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Título é obrigatório.");

            var v = value.Trim();

            if (v.Length > MaxLength)
                throw new DomainException($"Título excede o limite de {MaxLength} caracteres (atual: {v.Length}).");

            return new TicketTitle(v);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
