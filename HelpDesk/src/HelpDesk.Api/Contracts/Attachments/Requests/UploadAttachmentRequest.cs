namespace HelpDesk.Api.Contracts.Attachments.Requests
{
    public sealed class UploadAttachmentRequest
    {
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long SizeBytes { get; set; }
    }
}