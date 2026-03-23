using HelpDesk.Application.IdentityAccess.Ports;

namespace HelpDesk.UnitTests.Shared.Fakes
{
    public sealed class InMemoryUserReadPort : IUserReadPort
    {
        private readonly Dictionary<int, UserSnapshot> _users = new();

        public void Seed(UserSnapshot u) => _users[u.Id] = u;

        public Task<UserSnapshot?> GetByIdAsync(int id)
            => Task.FromResult(_users.TryGetValue(id, out var u) ? u : null);
    }
}
