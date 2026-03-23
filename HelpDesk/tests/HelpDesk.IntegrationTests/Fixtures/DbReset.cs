using HelpDesk.Infrastructure.Persistence;

namespace HelpDesk.IntegrationTests.Fixtures
{
    public static class DbReset
    {
        public static async Task ResetAsync(AppDbContext db)
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }
    }
}
