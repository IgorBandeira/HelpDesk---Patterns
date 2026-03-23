using FluentAssertions;
using HelpDesk.Api.Contracts.ServiceCatalog.Responses;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Fixtures;
using HelpDesk.IntegrationTests.ServiceCatalog.Seed;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace HelpDesk.IntegrationTests.ServiceCatalog
{
    public class Categories_GetById_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public Categories_GetById_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task GetById_Should_Return_404_When_Category_Not_Found()
        {
            var client = _factory.CreateClient();

            var resp = await client.GetAsync("/api/categories/999999");

            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task GetById_Should_Return_Category_When_Found()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var categoryId = await ServiceCatalogSeed.SeedCategoryAsync(db, "Hardware");

            var resp = await client.GetAsync($"/api/categories/{categoryId}");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var dto = await resp.Content.ReadFromJsonAsync<CategoryItemResponse>();
            Assert.NotNull(dto);

            dto!.Id.Should().Be(categoryId);
            dto.Name.Should().Be("Hardware");
            dto.ParentId.Should().BeNull();
        }

        [Fact]
        public async Task GetById_Should_Return_Parent_Dash_Name_When_Category_Is_Subcategory()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var parentId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");
            var childId = await ServiceCatalogSeed.SeedSubCategoryAsync(db, parentId, "Notebook");

            var resp = await client.GetAsync($"/api/categories/{childId}");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var dto = await resp.Content.ReadFromJsonAsync<CategoryItemResponse>();
            Assert.NotNull(dto);

            dto!.Id.Should().Be(childId);
            dto.Name.Should().Be("Hardware - Notebook");
            dto.ParentId.Should().Be(parentId);
        }
    }
}