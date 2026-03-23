using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Domain.IdentityAccess.Aggregates;
using HelpDesk.Domain.IdentityAccess.ValueObjects;
using HelpDesk.Infrastructure.IdentityAccess.Models;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.IdentityAccess.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db) => _db = db;

        public async Task<User?> GetByIdAsync(int id)
        {
            var e = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
            return e is null ? null : MapToDomain(e);
        }

        public async Task<User?> GetByIdNoTrackingAsync(int id)
        {
            var e = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return e is null ? null : MapToDomain(e);
        }

        public Task<bool> EmailExistsAsync(string emailLower, int? excludingUserId = null)
        {
            var q = _db.Users.AsNoTracking().Where(x => x.Email.ToLower() == emailLower);

            if (excludingUserId.HasValue)
                q = q.Where(x => x.Id != excludingUserId.Value);

            return q.AnyAsync();
        }

        public async Task AddAsync(User user)
        {
            var e = new UserEntity
            {
                Name = user.Name.Value,
                Email = user.Email.Value,
                Role = user.Role.Value
            };

            _db.Users.Add(e);
            await _db.SaveChangesAsync();

            typeof(User).BaseType!.BaseType!
                .GetProperty("Id")!
                .SetValue(user, e.Id);
        }

        public async Task SaveAsync(User user)
        {
            var e = await _db.Users.FirstOrDefaultAsync(x => x.Id == user.Id);
            if (e is null) return;

            e.Name = user.Name.Value;
            e.Email = user.Email.Value;
            e.Role = user.Role.Value;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(User user)
        {
            var e = await _db.Users.FirstOrDefaultAsync(x => x.Id == user.Id);
            if (e is null) return;

            _db.Users.Remove(e);
            await _db.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<UserListItem>> ListAsync(
            string? role,
            string? emailContainsLower,
            string? nameContainsLower,
            int skip,
            int take)
        {
            IQueryable<UserEntity> q = _db.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(role))
                q = q.Where(u => u.Role == role);

            if (!string.IsNullOrWhiteSpace(emailContainsLower))
                q = q.Where(u => u.Email.ToLower().Contains(emailContainsLower));

            if (!string.IsNullOrWhiteSpace(nameContainsLower))
                q = q.Where(u => u.Name.ToLower().Contains(nameContainsLower));

            var items = await q
                .OrderBy(u => u.Id)
                .Skip(skip)
                .Take(take)
                .Select(u => new UserListItem(u.Id, u.Name, u.Email, u.Role))
                .ToListAsync();

            return items;
        }

        private static User MapToDomain(UserEntity e)
        {
            var domain = User.CreateNew(
                UserName.Create(e.Name),
                EmailAddress.Create(e.Email),
                UserRole.Create(e.Role));

            typeof(User).BaseType!.BaseType!
                .GetProperty("Id")!
                .SetValue(domain, e.Id);

            return domain;
        }
    }
}
