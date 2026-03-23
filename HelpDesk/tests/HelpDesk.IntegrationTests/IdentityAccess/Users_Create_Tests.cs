using HelpDesk.Api.Contracts.IdentityAccess.Requests;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Fixtures;
using HelpDesk.IntegrationTests.IdentityAccess.Seed;
using HelpDesk.IntegrationTests.Utilities.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace HelpDesk.IntegrationTests.IdentityAccess
{
    public class Users_Create_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public Users_Create_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task Create_Should_Require_Manager()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);
            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "r@x.com", "Requester");

            var req = new CreateUserRequest { Name = "X", Email = "x@x.com", Role = "Requester" };

            var http = new HttpRequestMessage(HttpMethod.Post, "/api/users")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(requesterId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);

            var http2 = new HttpRequestMessage(HttpMethod.Post, "/api/users")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(managerId);

            var resp2 = await client.SendAsync(http2);
            Assert.Equal(HttpStatusCode.Created, resp2.StatusCode);
        }

        [Fact]
        public async Task Create_Should_Validate_Email_And_Role_And_Unique_Email_CaseInsensitive()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);

            var badEmail = new CreateUserRequest { Name = "X", Email = "bad", Role = "Requester" };
            var badHttp = new HttpRequestMessage(HttpMethod.Post, "/api/users")
            {
                Content = JsonContent.Create(badEmail)
            }.WithUserId(managerId);

            var badResp = await client.SendAsync(badHttp);
            Assert.Equal(HttpStatusCode.BadRequest, badResp.StatusCode);

            var badRole = new CreateUserRequest { Name = "X", Email = "x1@x.com", Role = "Admin" };
            var badRoleHttp = new HttpRequestMessage(HttpMethod.Post, "/api/users")
            {
                Content = JsonContent.Create(badRole)
            }.WithUserId(managerId);

            var badRoleResp = await client.SendAsync(badRoleHttp);
            Assert.Equal(HttpStatusCode.BadRequest, badRoleResp.StatusCode);

            var ok = new CreateUserRequest { Name = "X", Email = "dup@x.com", Role = "Requester" };
            var okHttp = new HttpRequestMessage(HttpMethod.Post, "/api/users")
            {
                Content = JsonContent.Create(ok)
            }.WithUserId(managerId);

            var okResp = await client.SendAsync(okHttp);
            Assert.Equal(HttpStatusCode.Created, okResp.StatusCode);

            var dup = new CreateUserRequest { Name = "Y", Email = "DUP@x.com", Role = "Requester" };
            var dupHttp = new HttpRequestMessage(HttpMethod.Post, "/api/users")
            {
                Content = JsonContent.Create(dup)
            }.WithUserId(managerId);

            var dupResp = await client.SendAsync(dupHttp);
            Assert.Equal(HttpStatusCode.Conflict, dupResp.StatusCode);
        }
    }
}
