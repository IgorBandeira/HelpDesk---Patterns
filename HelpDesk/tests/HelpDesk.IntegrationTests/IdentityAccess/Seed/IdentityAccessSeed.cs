using HelpDesk.Infrastructure.IdentityAccess.Models;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Ticketing.Models;

namespace HelpDesk.IntegrationTests.IdentityAccess.Seed
{
    public static class IdentityAccessSeed
    {
        public static async Task<int> SeedManagerAsync(AppDbContext db)
        {
            var unique = Guid.NewGuid().ToString("N")[..8];
            var m = new UserEntity { Name = "Manager", Email = $"m{unique}@x.com", Role = "Manager" };
            db.Users.Add(m);
            await db.SaveChangesAsync();
            return m.Id;
        }

        public static async Task<int> SeedUserAsync(AppDbContext db, string name, string email, string role)
        {
            var u = new UserEntity { Name = name, Email = email, Role = role };
            db.Users.Add(u);
            await db.SaveChangesAsync();
            return u.Id;
        }

        public static async Task SeedActiveTicketForRequesterAsync(AppDbContext db, int requesterId)
        {
            db.Tickets.Add(new TicketEntity
            {
                Title = "T",
                Status = "Novo",
                PriorityLevel = "Média",
                RequesterId = requesterId
            });
            await db.SaveChangesAsync();
        }
    }
}
