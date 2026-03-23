using HelpDesk.Api.Contracts.IdentityAccess.Requests;
using HelpDesk.Api.Contracts.IdentityAccess.Responses;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Fixtures;
using HelpDesk.IntegrationTests.IdentityAccess.Seed;
using HelpDesk.IntegrationTests.Utilities.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace HelpDesk.IntegrationTests.IdentityAccess
{
    public class Users_Patch_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public Users_Patch_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task Patch_Should_Require_Manager()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);
            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var targetId = await IdentityAccessSeed.SeedUserAsync(db, "T", "t@x.com", "Requester");

            var req = new UpdateUserRequest { Name = "New" };

            var http1 = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/users/{targetId}")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(requesterId);

            var resp1 = await client.SendAsync(http1);
            Assert.Equal(HttpStatusCode.Forbidden, resp1.StatusCode);

            var http2 = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/users/{targetId}")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(managerId);

            var resp2 = await client.SendAsync(http2);
            Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);
        }

        [Fact]
        public async Task Patch_Should_Return_404_When_User_Not_Found()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), "/api/users/999999")
            {
                Content = JsonContent.Create(new UpdateUserRequest { Name = "New" })
            }.WithUserId(managerId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task Patch_Should_Return_400_When_No_Changes_Detected()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);
            var targetId = await IdentityAccessSeed.SeedUserAsync(db, "T", "t@x.com", "Requester");

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/users/{targetId}")
            {
                Content = JsonContent.Create(new UpdateUserRequest { })
            }.WithUserId(managerId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Patch_Should_Validate_Email_And_Role_And_Email_Conflict()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);

            var u1 = await IdentityAccessSeed.SeedUserAsync(db, "A", "a@x.com", "Requester");
            var u2 = await IdentityAccessSeed.SeedUserAsync(db, "B", "b@x.com", "Requester");

            var badEmail = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/users/{u2}")
            {
                Content = JsonContent.Create(new UpdateUserRequest { Email = "bad" })
            }.WithUserId(managerId);

            var respBadEmail = await client.SendAsync(badEmail);
            Assert.Equal(HttpStatusCode.BadRequest, respBadEmail.StatusCode);

            var badRole = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/users/{u2}")
            {
                Content = JsonContent.Create(new UpdateUserRequest { Role = "Admin" })
            }.WithUserId(managerId);

            var respBadRole = await client.SendAsync(badRole);
            Assert.Equal(HttpStatusCode.BadRequest, respBadRole.StatusCode);

            var conflict = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/users/{u2}")
            {
                Content = JsonContent.Create(new UpdateUserRequest { Email = "A@x.com" })
            }.WithUserId(managerId);

            var respConflict = await client.SendAsync(conflict);
            Assert.Equal(HttpStatusCode.Conflict, respConflict.StatusCode);
        }

        [Fact]
        public async Task Patch_Should_Update_Name_Email_And_Role_When_Valid()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);
            var targetId = await IdentityAccessSeed.SeedUserAsync(db, "John", "john@x.com", "Requester");

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/users/{targetId}")
            {
                Content = JsonContent.Create(new UpdateUserRequest
                {
                    Name = "  Johnny  ",
                    Email = "johnny@x.com",
                    Role = "Agent"
                })
            }.WithUserId(managerId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var dto = await resp.Content.ReadFromJsonAsync<UserResponse>();
            Assert.NotNull(dto);

            Assert.Equal(targetId, dto!.Id);
            Assert.Equal("Johnny", dto.Name);
            Assert.Equal("johnny@x.com", dto.Email);
            Assert.Equal("Agent", dto.Role);
        }
    }
}
