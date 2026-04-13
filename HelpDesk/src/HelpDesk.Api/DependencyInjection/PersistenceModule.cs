using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Persistence.Adapters;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.DependencyInjection
{
    public static class PersistenceModule
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("HelpDesk_Postgre");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Connection string 'HelpDesk' não encontrada.");

            IDatabaseProviderAdapter adapter = new PostgreSqlDatabaseProvider();

            services.AddDbContext<AppDbContext>(options =>
                adapter.Configure(options, connectionString));

            return services;
        }
    }
}