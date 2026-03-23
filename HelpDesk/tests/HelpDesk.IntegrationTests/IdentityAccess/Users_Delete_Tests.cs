using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Fixtures;
using HelpDesk.IntegrationTests.IdentityAccess.Seed;
using HelpDesk.IntegrationTests.Utilities.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace HelpDesk.IntegrationTests.IdentityAccess
{

    public class Users_Delete_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public Users_Delete_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task Delete_Should_Require_Manager()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);
            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "r@x.com", "Requester");
            var targetId = await IdentityAccessSeed.SeedUserAsync(db, "T", "t@x.com", "Requester");

            var req1 = new HttpRequestMessage(HttpMethod.Delete, $"/api/users/{targetId}")
                .WithUserId(requesterId);

            var resp1 = await client.SendAsync(req1);
            Assert.Equal(HttpStatusCode.Forbidden, resp1.StatusCode);

            var req2 = new HttpRequestMessage(HttpMethod.Delete, $"/api/users/{targetId}")
                .WithUserId(managerId);

            var resp2 = await client.SendAsync(req2);
            Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);
        }

        [Fact]
        public async Task Delete_Should_Return_404_When_User_Not_Found()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);

            var req = new HttpRequestMessage(HttpMethod.Delete, "/api/users/999999")
                .WithUserId(managerId);

            var resp = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task Delete_Should_Block_When_User_Has_Active_Tickets()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);
            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "r@x.com", "Requester");
            await IdentityAccessSeed.SeedActiveTicketForRequesterAsync(db, requesterId);

            var req = new HttpRequestMessage(HttpMethod.Delete, $"/api/users/{requesterId}")
                .WithUserId(managerId);

            var resp = await client.SendAsync(req);

            Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        }
    }
}
