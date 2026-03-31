using HelpDesk.Domain.SharedKernel.Exceptions;
using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.Ticketing.ValueObjects
{
    public sealed class TicketDescription : ValueObject
    {
        public string Value { get; }

        private TicketDescription(string value) => Value = value;

        public static TicketDescription Create(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Descrição é obrigatória.");

            return new TicketDescription(value.Trim());
        }
        
            protected override IEnumerable<object?> GetEqualityComponents()
            {
                yield return Value;
            }
    }
}
