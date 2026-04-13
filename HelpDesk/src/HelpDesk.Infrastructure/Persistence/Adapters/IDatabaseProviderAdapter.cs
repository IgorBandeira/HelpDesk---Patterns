using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Persistence.Adapters
{
    public interface IDatabaseProviderAdapter
    {
        void Configure(DbContextOptionsBuilder optionsBuilder, string connectionString);
    }
}