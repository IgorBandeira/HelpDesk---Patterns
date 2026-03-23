using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Domain.Collaboration.ValueObjects
{
    public sealed class CommentMessage
    {
        public const int MaxChars = 4000;
        public string Value { get; }

        private CommentMessage(string value) => Value = value;

        public static CommentMessage Create(string? raw)
        {
            var message = (raw ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(message))
                throw new DomainException("Mensagem é obrigatória.");

            if (message.Length > MaxChars)
                throw new DomainException($"Mensagem excede o limite de {MaxChars} caracteres (atual: {message.Length}).");

            return new CommentMessage(message);
        }
    }
}
