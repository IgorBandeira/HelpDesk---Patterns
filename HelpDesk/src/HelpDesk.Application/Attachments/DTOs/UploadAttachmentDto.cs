namespace HelpDesk.Application.Attachments.DTOs
{
    public sealed record UploadAttachmentDto(
        string FileName,
        string? ContentType,
        long SizeBytes
    );
}
