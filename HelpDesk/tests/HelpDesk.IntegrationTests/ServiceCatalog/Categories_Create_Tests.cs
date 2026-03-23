using HelpDesk.Api.Contracts.ServiceCatalog.Requests;
using HelpDesk.Api.Contracts.ServiceCatalog.Responses;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Fixtures;
using HelpDesk.IntegrationTests.IdentityAccess.Seed;
using HelpDesk.IntegrationTests.ServiceCatalog.Seed;
using HelpDesk.IntegrationTests.Utilities.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace HelpDesk.IntegrationTests.ServiceCatalog
{
    public class Categories_Create_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public Categories_Create_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task Create_Should_Require_Manager()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);
            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Requester", "requester@x.com", "Requester");

            var req = new CreateCategoryRequest
            {
                Name = "Hardware",
                ParentId = null
            };

            var http1 = new HttpRequestMessage(HttpMethod.Post, "/api/categories")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(requesterId);

            var resp1 = await client.SendAsync(http1);
            Assert.Equal(HttpStatusCode.Forbidden, resp1.StatusCode);

            var http2 = new HttpRequestMessage(HttpMethod.Post, "/api/categories")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(managerId);

            var resp2 = await client.SendAsync(http2);
            Assert.Equal(HttpStatusCode.Created, resp2.StatusCode);
        }

        [Fact]
        public async Task Create_Should_Return_400_When_Name_Is_Missing()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);

            var req = new CreateCategoryRequest
            {
                Name = "   ",
                ParentId = null
            };

            var http = new HttpRequestMessage(HttpMethod.Post, "/api/categories")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(managerId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Create_Should_Return_400_When_Name_Exceeds_Max_Length()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);

            var req = new CreateCategoryRequest
            {
                Name = new string('A', 181),
                ParentId = null
            };

            var http = new HttpRequestMessage(HttpMethod.Post, "/api/categories")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(managerId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Create_Should_Trim_Name_When_Valid()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);

            var req = new CreateCategoryRequest
            {
                Name = "  Hardware  ",
                ParentId = null
            };

            var http = new HttpRequestMessage(HttpMethod.Post, "/api/categories")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(managerId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

            var dto = await resp.Content.ReadFromJsonAsync<CategoryItemResponse>();
            Assert.NotNull(dto);
            Assert.Equal("Hardware", dto!.Name);
        }

        [Fact]
        public async Task Create_Should_Return_Conflict_When_Name_Already_Exists()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);
            await ServiceCatalogSeed.SeedCategoryAsync(db, "Hardware");

            var req = new CreateCategoryRequest
            {
                Name = "Hardware",
                ParentId = null
            };

            var http = new HttpRequestMessage(HttpMethod.Post, "/api/categories")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(managerId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        }

        [Fact]
        public async Task Create_Should_Return_400_When_Parent_Does_Not_Exist()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);

            var req = new CreateCategoryRequest
            {
                Name = "Notebook",
                ParentId = 999999
            };

            var http = new HttpRequestMessage(HttpMethod.Post, "/api/categories")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(managerId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Create_Should_Return_Conflict_When_Parent_Is_Subcategory()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);

            var parentId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");
            var childId = await ServiceCatalogSeed.SeedSubCategoryAsync(db, parentId, "Notebook");

            var req = new CreateCategoryRequest
            {
                Name = "Mouse Gamer",
                ParentId = childId
            };

            var http = new HttpRequestMessage(HttpMethod.Post, "/api/categories")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(managerId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        }

        [Fact]
        public async Task Create_Should_Create_Subcategory_When_Parent_Is_Root()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);
            var parentId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var req = new CreateCategoryRequest
            {
                Name = "Notebook",
                ParentId = parentId
            };

            var http = new HttpRequestMessage(HttpMethod.Post, "/api/categories")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(managerId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

            var dto = await resp.Content.ReadFromJsonAsync<CategoryItemResponse>();
            Assert.NotNull(dto);
            Assert.Equal("Notebook", dto!.Name);
            Assert.Equal(parentId, dto.ParentId);
        }
    }
}