using HelpDesk.Application.Ticketing.Ports;

namespace HelpDesk.UnitTests.IdentityAccess.Fakes
{
    public sealed class FakeTicketUserQueryPortWithData : ITicketUserQueryPort
    {
        private readonly HashSet<int> _activeRequester = new();
        private readonly HashSet<int> _activeAssignee = new();

        private readonly Dictionary<int, IReadOnlyList<UserTicketListItem>> _requested = new();
        private readonly Dictionary<int, IReadOnlyList<UserTicketListItem>> _assigned = new();

        public void MarkActiveAsRequester(int userId) => _activeRequester.Add(userId);
        public void MarkActiveAsAssignee(int userId) => _activeAssignee.Add(userId);

        public void SetRequested(int userId, IReadOnlyList<UserTicketListItem> items)
            => _requested[userId] = items;

        public void SetAssigned(int userId, IReadOnlyList<UserTicketListItem> items)
            => _assigned[userId] = items;

        public Task<bool> HasActiveTicketsAsRequesterAsync(int userId)
            => Task.FromResult(_activeRequester.Contains(userId));

        public Task<bool> HasActiveTicketsAsAssigneeAsync(int userId)
            => Task.FromResult(_activeAssignee.Contains(userId));

        public Task<IReadOnlyList<UserTicketListItem>> ListRequestedTicketsAsync(int userId)
            => Task.FromResult(_requested.TryGetValue(userId, out var items)
                ? items
                : Array.Empty<UserTicketListItem>());

        public Task<IReadOnlyList<UserTicketListItem>> ListAssignedTicketsAsync(int userId)
            => Task.FromResult(_assigned.TryGetValue(userId, out var items)
                ? items
                : Array.Empty<UserTicketListItem>());
    }
}
