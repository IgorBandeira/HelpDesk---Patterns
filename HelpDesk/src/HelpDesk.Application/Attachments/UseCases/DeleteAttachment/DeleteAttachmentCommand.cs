namespace HelpDesk.Application.Attachments.UseCases.DeleteAttachment
{
    public sealed record DeleteAttachmentCommand(int TicketId, int AttachmentId, int UserId);
}
