using HelpDesk.Domain.SharedKernel.Exceptions;
using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.Attachments.ValueObjects
{
    public sealed class StorageKey : ValueObject
    {
        public string Value { get; }

        private StorageKey(string value) => Value = value;

        public static StorageKey Create(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Chave de armazenamento inválida.");

            return new StorageKey(value.Trim());
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
