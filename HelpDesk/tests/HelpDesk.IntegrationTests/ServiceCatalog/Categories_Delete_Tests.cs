using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Fixtures;
using HelpDesk.IntegrationTests.IdentityAccess.Seed;
using HelpDesk.IntegrationTests.ServiceCatalog.Seed;
using HelpDesk.IntegrationTests.Utilities.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace HelpDesk.IntegrationTests.ServiceCatalog
{
    public class Categories_Delete_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public Categories_Delete_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task Delete_Should_Require_Manager()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);
            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedCategoryAsync(db, "Hardware");

            var req1 = new HttpRequestMessage(HttpMethod.Delete, $"/api/categories/{categoryId}")
                .WithUserId(requesterId);

            var resp1 = await client.SendAsync(req1);
            Assert.Equal(HttpStatusCode.Forbidden, resp1.StatusCode);

            var req2 = new HttpRequestMessage(HttpMethod.Delete, $"/api/categories/{categoryId}")
                .WithUserId(managerId);

            var resp2 = await client.SendAsync(req2);
            Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);
        }

        [Fact]
        public async Task Delete_Should_Return_404_When_Category_Not_Found()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);

            var req = new HttpRequestMessage(HttpMethod.Delete, "/api/categories/999999")
                .WithUserId(managerId);

            var resp = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task Delete_Should_Block_When_Category_Has_Children()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);
            var parentId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");
            await ServiceCatalogSeed.SeedSubCategoryAsync(db, parentId, "Notebook");

            var req = new HttpRequestMessage(HttpMethod.Delete, $"/api/categories/{parentId}")
                .WithUserId(managerId);

            var resp = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        }

        [Fact]
        public async Task Delete_Should_Block_When_Category_Has_Active_Tickets()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);
            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "r@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedCategoryAsync(db, "Hardware");

            await ServiceCatalogSeed.SeedActiveTicketForCategoryAsync(db, categoryId, requesterId);

            var req = new HttpRequestMessage(HttpMethod.Delete, $"/api/categories/{categoryId}")
                .WithUserId(managerId);

            var resp = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        }

        [Fact]
        public async Task Delete_Should_Allow_When_Category_Has_Only_Closed_Tickets()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);
            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "r@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedCategoryAsync(db, "Hardware");

            await ServiceCatalogSeed.SeedClosedTicketForCategoryAsync(db, categoryId, requesterId);

            var req = new HttpRequestMessage(HttpMethod.Delete, $"/api/categories/{categoryId}")
                .WithUserId(managerId);

            var resp = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        [Fact]
        public async Task Delete_Should_Allow_When_Category_Has_Only_Cancelled_Tickets()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);
            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "r@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedCategoryAsync(db, "Hardware");

            await ServiceCatalogSeed.SeedCancelledTicketForCategoryAsync(db, categoryId, requesterId);

            var req = new HttpRequestMessage(HttpMethod.Delete, $"/api/categories/{categoryId}")
                .WithUserId(managerId);

            var resp = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }
    }
}