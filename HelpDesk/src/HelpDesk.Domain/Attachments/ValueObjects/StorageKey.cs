using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Domain.Attachments.ValueObjects
{
    public sealed class StorageKey
    {
        public string Value { get; }

        private StorageKey(string value) => Value = value;

        public static StorageKey Create(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Chave de armazenamento inválida.");

            return new StorageKey(value.Trim());
        }

        public override string ToString() => Value;
    }
}
