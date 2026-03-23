using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.IdentityAccess.Queries
{
    public sealed class UserReadPort : IUserReadPort
    {
        private readonly AppDbContext _db;

        public UserReadPort(AppDbContext db) => _db = db;

        public async Task<UserSnapshot?> GetByIdAsync(int id)
        {
            return await _db.Users
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new UserSnapshot(x.Id, x.Name, x.Email, x.Role))
                .FirstOrDefaultAsync();
        }
    }
}