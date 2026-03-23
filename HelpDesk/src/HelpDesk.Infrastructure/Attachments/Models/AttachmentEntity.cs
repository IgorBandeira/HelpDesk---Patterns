using HelpDesk.Infrastructure.IdentityAccess.Models;

namespace HelpDesk.Infrastructure.Attachments.Models
{
    public sealed class AttachmentEntity
    {
        public int Id { get; set; }
        public int TicketId { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }

        public string StorageKey { get; set; } = string.Empty;
        public string? PublicUrl { get; set; }

        public int? UploadedById { get; set; }
        public UserEntity? UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}