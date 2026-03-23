namespace HelpDesk.Application.Attachments.DTOs
{
    public sealed record AttachmentResponseDto(
        int Id,
        int TicketId,
        string FileName,
        string ContentType,
        long SizeBytes,
        string StorageKey,
        string? PublicUrl,
        DateTime UploadedAt,
        int UploadedById
    );
}
