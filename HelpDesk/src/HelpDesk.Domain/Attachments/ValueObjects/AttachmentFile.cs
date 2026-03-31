using HelpDesk.Domain.Attachments.Rules;
using HelpDesk.Domain.SharedKernel.Exceptions;
using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.Attachments.ValueObjects
{
    public sealed class AttachmentFile : ValueObject
    {
        public string FileName { get; }
        public string ContentType { get; }
        public long SizeBytes { get; }

        private AttachmentFile(string fileName, string contentType, long sizeBytes)
        {
            FileName = fileName;
            ContentType = contentType;
            SizeBytes = sizeBytes;
        }

        public static AttachmentFile Create(string? fileName, string? contentType, long sizeBytes)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new DomainException("Arquivo inválido.");

            AttachmentRules.ValidateFileSize(sizeBytes);
            AttachmentRules.ValidateExtension(fileName);

            var ct = string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType!;

            return new AttachmentFile(fileName, ct, sizeBytes);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return FileName;
            yield return ContentType;
            yield return SizeBytes;
        }
    }
}
