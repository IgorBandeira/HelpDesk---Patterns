namespace HelpDesk.Application.Attachments.UseCases.GetAttachmentById
{
    public sealed record GetAttachmentByIdQuery(int TicketId, int AttachmentId);
}
