using HelpDesk.Infrastructure.Persistence.Adapters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HelpDesk.Infrastructure.Persistence
{
    public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("HelpDesk_Postgre");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Connection string 'HelpDesk' não encontrada.");

            IDatabaseProviderAdapter adapter = new PostgreSqlDatabaseProvider();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            adapter.Configure(optionsBuilder, connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}