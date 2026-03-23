using HelpDesk.Application.Ticketing.Ports;

namespace HelpDesk.UnitTests.Shared.Fakes
{
    public sealed class InMemoryTicketReadPort : ITicketReadPort
    {
        private readonly Dictionary<int, TicketSnapshot> _tickets = new();
        public void Seed(TicketSnapshot t) => _tickets[t.Id] = t;

        public Task<TicketSnapshot?> GetByIdAsync(int ticketId)
            => Task.FromResult(_tickets.TryGetValue(ticketId, out var t) ? t : null);
    }
}
