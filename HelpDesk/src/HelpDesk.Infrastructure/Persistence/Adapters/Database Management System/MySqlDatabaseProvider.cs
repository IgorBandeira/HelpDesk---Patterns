using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Persistence.Adapters
{
    public sealed class MySqlDatabaseProvider : IDatabaseProviderAdapter
    {
        public void Configure(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            optionsBuilder.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString));
        }
    }
}