using HelpDesk.Domain.SharedKernel.Exceptions;
using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.IdentityAccess.ValueObjects
{
    public sealed class UserName : ValueObject
    {
        public string Value { get; }

        private UserName(string value) => Value = value;

        public static UserName Create(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                throw new DomainException("Nome é obrigatório.");

            var name = raw.Trim();

            return new UserName(name);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
