using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Domain.Attachments.Aggregates;

namespace HelpDesk.UnitTests.Attachments.Fakes
{
    public sealed class InMemoryAttachmentRepository : IAttachmentRepository
    {
        private readonly List<Attachment> _items = new();
        private int _nextId = 1;

        public Task AddAsync(Attachment attachment)
        {
            typeof(Attachment).GetProperty("Id")!.SetValue(attachment, _nextId++);
            _items.Add(attachment);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Attachment attachment)
        {
            _items.RemoveAll(x => x.Id == attachment.Id && x.TicketId == attachment.TicketId);
            return Task.CompletedTask;
        }

        public Task<Attachment?> GetByIdAsync(int ticketId, int attachmentId)
            => Task.FromResult(_items.FirstOrDefault(a => a.TicketId == ticketId && a.Id == attachmentId));

        public Task<IReadOnlyList<Attachment>> ListByTicketAsync(int ticketId)
            => Task.FromResult((IReadOnlyList<Attachment>)_items.Where(a => a.TicketId == ticketId).ToList());
    }
}
