using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Domain.Ticketing.ValueObjects
{
    public sealed class TicketTitle
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
    }
}
