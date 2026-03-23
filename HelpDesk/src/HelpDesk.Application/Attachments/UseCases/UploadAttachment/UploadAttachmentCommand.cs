using HelpDesk.Application.Attachments.Ports;

namespace HelpDesk.Application.Attachments.UseCases.UploadAttachment
{
    public sealed record UploadAttachmentCommand(int TicketId, int UserId, UploadFile File);
}