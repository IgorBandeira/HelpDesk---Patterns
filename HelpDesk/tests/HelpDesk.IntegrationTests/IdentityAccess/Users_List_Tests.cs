using HelpDesk.Api.Contracts.IdentityAccess.Responses;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Fixtures;
using HelpDesk.IntegrationTests.IdentityAccess.Seed;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace HelpDesk.IntegrationTests.IdentityAccess
{
    public class Users_List_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public Users_List_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task List_Should_Return_400_When_Role_Invalid()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/api/users?role=Admin");
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task List_Should_Filter_By_Role_Email_Name_And_Paginate()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            await IdentityAccessSeed.SeedUserAsync(db, "John", "john@x.com", "Requester");
            await IdentityAccessSeed.SeedUserAsync(db, "Johnny", "johnny@x.com", "Agent");
            await IdentityAccessSeed.SeedUserAsync(db, "Mary", "mary@x.com", "Manager");
            await IdentityAccessSeed.SeedUserAsync(db, "Joana", "joana@x.com", "Requester");

            var byRole = await client.GetFromJsonAsync<List<UserResponse>>("/api/users?role=Requester&page=1&pageSize=20");
            Assert.NotNull(byRole);
            Assert.All(byRole!, u => Assert.Equal("Requester", u.Role));

            var byEmail = await client.GetFromJsonAsync<List<UserResponse>>("/api/users?email=JOHN&page=1&pageSize=20");
            Assert.NotNull(byEmail);
            Assert.True(byEmail!.Count >= 2);

            var byName = await client.GetFromJsonAsync<List<UserResponse>>("/api/users?name=jo&page=1&pageSize=20");
            Assert.NotNull(byName);
            Assert.True(byName!.Count >= 2);

            var page1 = await client.GetFromJsonAsync<List<UserResponse>>("/api/users?page=1&pageSize=2");
            var page2 = await client.GetFromJsonAsync<List<UserResponse>>("/api/users?page=2&pageSize=2");

            Assert.NotNull(page1);
            Assert.NotNull(page2);
            Assert.Equal(2, page1!.Count);
            Assert.Equal(2, page2!.Count);
            Assert.Empty(page1.Select(x => x.Id).Intersect(page2.Select(x => x.Id)));
        }
    }
}
