using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Domain.Ticketing.ValueObjects
{
    public sealed class TicketDescription
    {
        public string Value { get; }

        private TicketDescription(string value) => Value = value;

        public static TicketDescription Create(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Descrição é obrigatória.");

            return new TicketDescription(value.Trim());
        }
    }
}
