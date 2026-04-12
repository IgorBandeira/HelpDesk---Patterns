using HelpDesk.Api.DependencyInjection;
using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Application.Operations.Ports.Email;
using HelpDesk.Infrastructure.Attachments.DependencyInjection;
using HelpDesk.Infrastructure.Collaboration.DependencyInjection;
using HelpDesk.Infrastructure.IdentityAccess.DependencyInjection;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.ServiceCatalog.DependencyInjection;
using HelpDesk.Infrastructure.Shared.DependencyInjection;
using HelpDesk.Infrastructure.Ticketing.DependencyInjection;
using HelpDesk.IntegrationTests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HelpDesk.IntegrationTests.Fixtures
{
    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? _connection;

        public FakeFileStoragePort FileStorageFake { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor is not null)
                    services.Remove(descriptor);

                _connection = new SqliteConnection("Data Source=:memory:");
                _connection.Open();

                services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(_connection));

                services.AddIdentityAccessInfrastructure();
                services.AddIdentityAccessApi();

                services.AddServiceCatalogInfrastructure();
                services.AddServiceCatalogApi();

                services.AddTicketingInfrastructure();
                services.AddTicketingApi();

                services.AddCollaborationInfrastructure();
                services.AddCollaborationApi();

                services.AddAttachmentsInfrastructure();
                services.AddAttachmentsApi();

                services.AddSharedInfrastructure();

                services.RemoveAll<IEmailSender>();
                services.AddScoped<IEmailSender, FakeEmailSender>();

                services.RemoveAll<IFileStoragePort>();
                services.AddSingleton(FileStorageFake);
                services.AddSingleton<IFileStoragePort>(sp => sp.GetRequiredService<FakeFileStoragePort>());

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _connection?.Dispose();
        }
    }
}