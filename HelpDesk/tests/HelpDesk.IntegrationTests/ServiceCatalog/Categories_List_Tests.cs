using HelpDesk.Api.Contracts.ServiceCatalog.Responses;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Fixtures;
using HelpDesk.IntegrationTests.ServiceCatalog.Seed;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace HelpDesk.IntegrationTests.ServiceCatalog
{
    public class Categories_List_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public Categories_List_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task List_Should_Return_All_Categories_When_No_Filter()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            await ServiceCatalogSeed.SeedCategoryAsync(db, "Hardware");
            await ServiceCatalogSeed.SeedCategoryAsync(db, "Software");
            await ServiceCatalogSeed.SeedCategoryAsync(db, "Rede");

            var items = await client.GetFromJsonAsync<List<CategoryItemResponse>>("/api/categories?page=1&pageSize=20");

            Assert.NotNull(items);
            Assert.Equal(13, items!.Count);
        }

        [Fact]
        public async Task List_Should_Filter_By_NameContains()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            await ServiceCatalogSeed.SeedCategoryAsync(db, "Hardware");
            await ServiceCatalogSeed.SeedCategoryAsync(db, "Software");
            await ServiceCatalogSeed.SeedCategoryAsync(db, "Hard Reset");

            var items = await client.GetFromJsonAsync<List<CategoryItemResponse>>("/api/categories?nameContains=Hard&page=1&pageSize=20");

            Assert.NotNull(items);
            Assert.Equal(2, items!.Count);
            Assert.All(items, x => Assert.Contains("Hard", x.Name));
        }

        [Fact]
        public async Task List_Should_Filter_By_ParentId()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var hardwareId = await ServiceCatalogSeed.SeedCategoryAsync(db, "Hardware");
            var softwareId = await ServiceCatalogSeed.SeedCategoryAsync(db, "Software");

            await ServiceCatalogSeed.SeedCategoryAsync(db, "Notebook", hardwareId);
            await ServiceCatalogSeed.SeedCategoryAsync(db, "Impressora", hardwareId);
            await ServiceCatalogSeed.SeedCategoryAsync(db, "ERP", softwareId);

            var items = await client.GetFromJsonAsync<List<CategoryItemResponse>>($"/api/categories?parentId={hardwareId}&page=1&pageSize=20");

            Assert.NotNull(items);
            Assert.Equal(2, items!.Count);
            Assert.All(items, x => Assert.Equal(hardwareId, x.ParentId));
        }

        [Fact]
        public async Task List_Should_Paginate()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            await ServiceCatalogSeed.SeedCategoryAsync(db, "A");
            await ServiceCatalogSeed.SeedCategoryAsync(db, "B");
            await ServiceCatalogSeed.SeedCategoryAsync(db, "C");
            await ServiceCatalogSeed.SeedCategoryAsync(db, "D");

            var page1 = await client.GetFromJsonAsync<List<CategoryItemResponse>>("/api/categories?page=1&pageSize=2");
            var page2 = await client.GetFromJsonAsync<List<CategoryItemResponse>>("/api/categories?page=2&pageSize=2");

            Assert.NotNull(page1);
            Assert.NotNull(page2);
            Assert.Equal(2, page1!.Count);
            Assert.Equal(2, page2!.Count);
            Assert.Empty(page1.Select(x => x.Id).Intersect(page2.Select(x => x.Id)));
        }

        [Fact]
        public async Task List_Should_Return_Parent_Dash_Name_When_Subcategory()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var parentId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");
            await ServiceCatalogSeed.SeedSubCategoryAsync(db, parentId, "Notebook");

            var items = await client.GetFromJsonAsync<List<CategoryItemResponse>>("/api/categories?page=1&pageSize=20");

            Assert.NotNull(items);
            Assert.Contains(items!, x => x.Name == "Hardware - Notebook");
        }

        [Fact]
        public async Task List_Should_Return_Empty_When_No_Items_Match_Filter()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            await ServiceCatalogSeed.SeedCategoryAsync(db, "Hardware");
            await ServiceCatalogSeed.SeedCategoryAsync(db, "Software");

            var items = await client.GetFromJsonAsync<List<CategoryItemResponse>>("/api/categories?nameContains=Inexistente&page=1&pageSize=20");

            Assert.NotNull(items);
            Assert.Empty(items!);
        }
    }
}