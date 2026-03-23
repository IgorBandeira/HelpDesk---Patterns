using HelpDesk.Application.Attachments.DTOs;

namespace HelpDesk.Application.Attachments.Ports
{
    public interface IAttachmentReadPort
    {
        Task<AttachmentListItemDto?> GetDetailsByIdAsync(int ticketId, int attachmentId);
        Task<IReadOnlyList<AttachmentListItemDto>> ListDetailsByTicketAsync(int ticketId);
    }
}
