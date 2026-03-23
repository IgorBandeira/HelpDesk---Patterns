using HelpDesk.Application.Collaboration.Ports;
using HelpDesk.Domain.Collaboration.Aggregates;

namespace HelpDesk.UnitTests.Collaboration.Fakes
{
    public sealed class InMemoryCommentRepository : ICommentRepository
    {
        private readonly List<TicketComment> _items = new();
        private int _nextId = 1;

        public Task<TicketComment?> GetByIdAsync(int ticketId, int commentId)
            => Task.FromResult(_items.FirstOrDefault(x => x.TicketId == ticketId && x.Id == commentId));

        public Task AddAsync(TicketComment comment)
        {
            typeof(TicketComment).GetProperty("Id")!.SetValue(comment, _nextId++);
            _items.Add(comment);
            return Task.CompletedTask;
        }

        public Task SaveAsync(TicketComment comment) => Task.CompletedTask;

        public Task DeleteAsync(TicketComment comment)
        {
            _items.RemoveAll(x => x.Id == comment.Id && x.TicketId == comment.TicketId);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TicketComment>> ListByTicketAsync(int ticketId)
            => Task.FromResult((IReadOnlyList<TicketComment>)_items.Where(x => x.TicketId == ticketId).ToList());
    }
}
