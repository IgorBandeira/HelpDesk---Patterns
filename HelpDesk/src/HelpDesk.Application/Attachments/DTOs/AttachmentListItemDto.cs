using HelpDesk.Application.Shared.DTOs;

namespace HelpDesk.Application.Attachments.DTOs
{
    public sealed record AttachmentListItemDto(
        int Id,
        int TicketId,
        string FileName,
        string ContentType,
        long SizeBytes,
        string StorageKey,
        string? PublicUrl,
        DateTime UploadedAt,
        UserMiniDto UploadedBy
    );
}
