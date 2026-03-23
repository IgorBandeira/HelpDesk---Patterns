using HelpDesk.Api.Contracts.Ticketing.Responses;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Fixtures;
using HelpDesk.IntegrationTests.IdentityAccess.Seed;
using HelpDesk.IntegrationTests.ServiceCatalog.Seed;
using HelpDesk.IntegrationTests.Ticketing.Seed;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace HelpDesk.IntegrationTests.Ticketing
{
    public class Tickets_List_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public Tickets_List_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task List_Should_Filter_By_Status()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            await TicketingSeed.SeedTicketAsync(db, "T1", "D1", TicketStatus.Novo, TicketPriority.Media, requesterId, categoryId);
            await TicketingSeed.SeedTicketAsync(db, "T2", "D2", TicketStatus.Resolvido, TicketPriority.Alta, requesterId, categoryId);

            var resp = await client.GetAsync("/api/tickets?status=Novo&page=1&pageSize=20");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var items = await resp.Content.ReadFromJsonAsync<List<TicketListItemResponse>>();
            Assert.NotNull(items);
            Assert.Single(items!);
            Assert.All(items!, x => Assert.Equal(TicketStatus.Novo, x.Status));
        }

        [Fact]
        public async Task List_Should_Return_400_When_Status_Is_Invalid()
        {
            var client = _factory.CreateClient();

            var resp = await client.GetAsync("/api/tickets?status=QualquerCoisa&page=1&pageSize=20");

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task List_Should_Filter_By_Priority()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            await TicketingSeed.SeedTicketAsync(db, "T1", "D1", TicketStatus.Novo, TicketPriority.Baixa, requesterId, categoryId);
            await TicketingSeed.SeedTicketAsync(db, "T2", "D2", TicketStatus.Novo, TicketPriority.Alta, requesterId, categoryId);

            var items = await client.GetFromJsonAsync<List<TicketListItemResponse>>($"/api/tickets?priority={Uri.EscapeDataString(TicketPriority.Alta)}&page=1&pageSize=20");

            Assert.NotNull(items);
            Assert.Single(items!);
            Assert.All(items!, x => Assert.Equal(TicketPriority.Alta, x.Priority));
        }

        [Fact]
        public async Task List_Should_Filter_By_Title()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            await TicketingSeed.SeedTicketAsync(db, "Falha no login", "D1", TicketStatus.Novo, TicketPriority.Media, requesterId, categoryId);
            await TicketingSeed.SeedTicketAsync(db, "Erro no banco", "D2", TicketStatus.Novo, TicketPriority.Media, requesterId, categoryId);

            var items = await client.GetFromJsonAsync<List<TicketListItemResponse>>("/api/tickets?title=login&page=1&pageSize=20");

            Assert.NotNull(items);
            Assert.Single(items!);
            Assert.Contains("login", items![0].Title, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task List_Should_Filter_By_RequesterId()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requester1 = await IdentityAccessSeed.SeedUserAsync(db, "Req1", "req1@x.com", "Requester");
            var requester2 = await IdentityAccessSeed.SeedUserAsync(db, "Req2", "req2@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            await TicketingSeed.SeedTicketAsync(db, "T1", "D1", TicketStatus.Novo, TicketPriority.Media, requester1, categoryId);
            await TicketingSeed.SeedTicketAsync(db, "T2", "D2", TicketStatus.Novo, TicketPriority.Media, requester2, categoryId);

            var items = await client.GetFromJsonAsync<List<TicketListItemResponse>>($"/api/tickets?requesterId={requester1}&page=1&pageSize=20");

            Assert.NotNull(items);
            Assert.Single(items!);
            Assert.All(items!, x => Assert.Equal(requester1, x.Requester.Id));
        }

        [Fact]
        public async Task List_Should_Filter_By_AssigneeId()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requester = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var agent1 = await IdentityAccessSeed.SeedUserAsync(db, "Agent1", "agent1@x.com", "Agent");
            var agent2 = await IdentityAccessSeed.SeedUserAsync(db, "Agent2", "agent2@x.com", "Agent");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            await TicketingSeed.SeedTicketAsync(db, "T1", "D1", TicketStatus.EmAnalise, TicketPriority.Media, requester, categoryId, agent1);
            await TicketingSeed.SeedTicketAsync(db, "T2", "D2", TicketStatus.EmAnalise, TicketPriority.Media, requester, categoryId, agent2);

            var items = await client.GetFromJsonAsync<List<TicketListItemResponse>>($"/api/tickets?assigneeId={agent1}&page=1&pageSize=20");

            Assert.NotNull(items);
            Assert.Single(items!);
            Assert.All(items!, x => Assert.Equal(agent1, x.Assignee!.Id));
        }

        [Fact]
        public async Task List_Should_Filter_By_CategoryId()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requester = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var cat1 = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");
            var cat2 = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Software");

            await TicketingSeed.SeedTicketAsync(db, "T1", "D1", TicketStatus.Novo, TicketPriority.Media, requester, cat1);
            await TicketingSeed.SeedTicketAsync(db, "T2", "D2", TicketStatus.Novo, TicketPriority.Media, requester, cat2);

            var items = await client.GetFromJsonAsync<List<TicketListItemResponse>>($"/api/tickets?categoryId={cat1}&page=1&pageSize=20");

            Assert.NotNull(items);
            Assert.Single(items!);
            Assert.All(items!, x => Assert.Equal(cat1, x.Category.Id));
        }

        [Fact]
        public async Task List_Should_Paginate()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requester = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            await TicketingSeed.SeedTicketAsync(db, "T1", "D1", TicketStatus.Novo, TicketPriority.Media, requester, categoryId);
            await TicketingSeed.SeedTicketAsync(db, "T2", "D2", TicketStatus.Novo, TicketPriority.Media, requester, categoryId);
            await TicketingSeed.SeedTicketAsync(db, "T3", "D3", TicketStatus.Novo, TicketPriority.Media, requester, categoryId);
            await TicketingSeed.SeedTicketAsync(db, "T4", "D4", TicketStatus.Novo, TicketPriority.Media, requester, categoryId);

            var page1 = await client.GetFromJsonAsync<List<TicketListItemResponse>>("/api/tickets?page=1&pageSize=2");
            var page2 = await client.GetFromJsonAsync<List<TicketListItemResponse>>("/api/tickets?page=2&pageSize=2");

            Assert.NotNull(page1);
            Assert.NotNull(page2);
            Assert.Equal(2, page1!.Count);
            Assert.Equal(2, page2!.Count);
            Assert.Empty(page1.Select(x => x.Id).Intersect(page2.Select(x => x.Id)));
        }

        [Fact]
        public async Task List_Should_Return_Empty_When_No_Items_Match()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var items = await client.GetFromJsonAsync<List<TicketListItemResponse>>("/api/tickets?title=inexistente&page=1&pageSize=20");

            Assert.NotNull(items);
            Assert.Empty(items!);
        }
    }
}