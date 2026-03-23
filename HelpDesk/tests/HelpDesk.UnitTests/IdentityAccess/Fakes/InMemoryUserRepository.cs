using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Domain.IdentityAccess.Aggregates;
using HelpDesk.Domain.IdentityAccess.ValueObjects;

namespace HelpDesk.UnitTests.IdentityAccess.Fakes
{
    public sealed class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();
        private int _nextId = 1;

        public User Seed(string name, string email, string role)
        {
            var u = User.CreateNew(
                UserName.Create(name),
                EmailAddress.Create(email),
                UserRole.Create(role));

            typeof(User).BaseType!.BaseType!
                .GetProperty("Id")!
                .SetValue(u, _nextId++);

            _users.Add(u);
            return u;
        }

        public Task<User?> GetByIdAsync(int id)
            => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

        public Task<User?> GetByIdNoTrackingAsync(int id)
            => GetByIdAsync(id);

        public Task<bool> EmailExistsAsync(string emailLower, int? excludingUserId = null)
        {
            var exists = _users.Any(u =>
                u.Email.Value.Trim().ToLowerInvariant() == emailLower &&
                (!excludingUserId.HasValue || u.Id != excludingUserId.Value));

            return Task.FromResult(exists);
        }

        public Task AddAsync(User user)
        {
            typeof(User).BaseType!.BaseType!
                .GetProperty("Id")!
                .SetValue(user, _nextId++);

            _users.Add(user);
            return Task.CompletedTask;
        }

        public Task SaveAsync(User user)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(User user)
        {
            _users.RemoveAll(x => x.Id == user.Id);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<UserListItem>> ListAsync(string? role, string? emailContainsLower, string? nameContainsLower, int skip, int take)
        {
            IEnumerable<User> q = _users;

            if (!string.IsNullOrWhiteSpace(role))
                q = q.Where(u => u.Role.Value == role);

            if (!string.IsNullOrWhiteSpace(emailContainsLower))
                q = q.Where(u => u.Email.Value.ToLowerInvariant().Contains(emailContainsLower));

            if (!string.IsNullOrWhiteSpace(nameContainsLower))
                q = q.Where(u => u.Name.Value.ToLowerInvariant().Contains(nameContainsLower));

            var items = q
                .OrderBy(u => u.Id)
                .Skip(skip)
                .Take(take)
                .Select(u => new UserListItem(u.Id, u.Name.Value, u.Email.Value, u.Role.Value))
                .ToList();

            return Task.FromResult((IReadOnlyList<UserListItem>)items);
        }
    }
}
