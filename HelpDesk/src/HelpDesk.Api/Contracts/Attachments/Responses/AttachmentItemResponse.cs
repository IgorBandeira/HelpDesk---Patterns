using HelpDesk.Api.Contracts.Shared;

namespace HelpDesk.Api.Contracts.Attachments.Responses
{
    public sealed class AttachmentItemResponse
    {
        public int Id { get; init; }
        public int TicketId { get; init; }
        public string FileName { get; init; } = string.Empty;
        public string ContentType { get; init; } = string.Empty;
        public long SizeBytes { get; init; }
        public string StorageKey { get; init; } = string.Empty;
        public string? PublicUrl { get; init; }
        public DateTime UploadedAt { get; init; }
        public UserMiniResponse UploadedBy { get; set; } = new();

    }
}