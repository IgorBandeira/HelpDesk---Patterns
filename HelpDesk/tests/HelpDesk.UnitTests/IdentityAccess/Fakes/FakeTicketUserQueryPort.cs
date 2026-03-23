using HelpDesk.Application.Ticketing.Ports;

namespace HelpDesk.UnitTests.IdentityAccess.Fakes
{
    public sealed class FakeTicketUserQueryPort : ITicketUserQueryPort
    {
        private readonly HashSet<int> _activeRequester = new();
        private readonly HashSet<int> _activeAssignee = new();

        public void MarkActiveAsRequester(int userId) => _activeRequester.Add(userId);
        public void MarkActiveAsAssignee(int userId) => _activeAssignee.Add(userId);

        public Task<bool> HasActiveTicketsAsRequesterAsync(int userId)
            => Task.FromResult(_activeRequester.Contains(userId));

        public Task<bool> HasActiveTicketsAsAssigneeAsync(int userId)
            => Task.FromResult(_activeAssignee.Contains(userId));

        public Task<IReadOnlyList<UserTicketListItem>> ListRequestedTicketsAsync(int userId)
            => Task.FromResult((IReadOnlyList<UserTicketListItem>)Array.Empty<UserTicketListItem>());

        public Task<IReadOnlyList<UserTicketListItem>> ListAssignedTicketsAsync(int userId)
            => Task.FromResult((IReadOnlyList<UserTicketListItem>)Array.Empty<UserTicketListItem>());
    }
}
