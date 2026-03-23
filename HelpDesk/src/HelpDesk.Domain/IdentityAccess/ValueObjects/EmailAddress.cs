using HelpDesk.Domain.SharedKernel.Exceptions;
using HelpDesk.Domain.SharedKernel.Primitives;
using System.Text.RegularExpressions;

namespace HelpDesk.Domain.IdentityAccess.ValueObjects
{
    public sealed class EmailAddress : ValueObject
    {
        private const string EmailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        public string Value { get; }

        private EmailAddress(string value) => Value = value;

        public static EmailAddress Create(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                throw new DomainException("E-mail é obrigatório.");

            var email = raw.Trim();

            if (!Regex.IsMatch(email, EmailRegex, RegexOptions.IgnoreCase))
                throw new DomainException("Formato de e-mail inválido.");

            return new EmailAddress(email);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value.ToLowerInvariant();
        }

        public override string ToString() => Value;
    }
}
