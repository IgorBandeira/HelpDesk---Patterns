using HelpDesk.Domain.Attachments.Aggregates;

namespace HelpDesk.Application.Attachments.Ports
{
    public interface IAttachmentRepository
    {
        Task AddAsync(Attachment attachment);
        Task DeleteAsync(Attachment attachment);

        Task<Attachment?> GetByIdAsync(int ticketId, int attachmentId);
        Task<IReadOnlyList<Attachment>> ListByTicketAsync(int ticketId);
    }
}
