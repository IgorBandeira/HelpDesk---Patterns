using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Domain.Attachments.Rules
{
    public static class AttachmentRules
    {
        public const long MaxBytes = 10 * 1024 * 1024;
        private static readonly HashSet<string> Blocked = new(StringComparer.OrdinalIgnoreCase)
    { ".exe", ".bat", ".sh" };

        public static void ValidateFileSize(long sizeBytes)
        {
            if (sizeBytes <= 0)
                throw new DomainException("Arquivo inválido.");

            if (sizeBytes > MaxBytes)
                throw new DomainException("Máx 10MB.");
        }

        public static void ValidateExtension(string fileName)
        {
            var ext = Path.GetExtension(fileName);
            if (!string.IsNullOrEmpty(ext) && Blocked.Contains(ext))
                throw new DomainException("Extensão proibida.");
        }
    }
}
