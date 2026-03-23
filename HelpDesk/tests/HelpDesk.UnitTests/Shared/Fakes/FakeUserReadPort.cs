using HelpDesk.Application.IdentityAccess.Ports;

namespace HelpDesk.UnitTests.IdentityAccess.Fakes
{
    public sealed class FakeUserReadPort : IUserReadPort
    {
        private readonly Dictionary<int, UserSnapshot> _users = new();

        public void Seed(int id, string name, string email, string role)
        {
            _users[id] = new UserSnapshot(id, name, email, role);
        }

        public Task<UserSnapshot?> GetByIdAsync(int id)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }
    }
}