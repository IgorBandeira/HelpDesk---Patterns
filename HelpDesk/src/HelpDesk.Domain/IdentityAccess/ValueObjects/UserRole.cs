using HelpDesk.Domain.IdentityAccess.Rules;
using HelpDesk.Domain.SharedKernel.Exceptions;
using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.IdentityAccess.ValueObjects
{
    public sealed class UserRole : ValueObject
    {
        public string Value { get; }

        private UserRole(string value) => Value = value;

        public static UserRole Create(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                throw new DomainException("Papel é obrigatório.");

            var role = raw.Trim();

            if (!UserRules.IsAllowedRole(role))
                throw new DomainException("Papel inválido (Requester, Agent, Manager).");

            return new UserRole(role);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value.ToLowerInvariant();
        }

        public override string ToString() => Value;
    }
}
