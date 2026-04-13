using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Persistence.Adapters
{
    public sealed class PostgreSqlDatabaseProvider : IDatabaseProviderAdapter
    {
        public void Configure(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}