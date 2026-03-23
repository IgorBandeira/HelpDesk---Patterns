using HelpDesk.Api.Contracts.Ticketing.Requests;
using HelpDesk.Api.Contracts.Ticketing.Responses;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Fixtures;
using HelpDesk.IntegrationTests.IdentityAccess.Seed;
using HelpDesk.IntegrationTests.ServiceCatalog.Seed;
using HelpDesk.IntegrationTests.Ticketing.Seed;
using HelpDesk.IntegrationTests.Utilities.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace HelpDesk.IntegrationTests.Ticketing
{
    public class Tickets_Update_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public Tickets_Update_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task Update_Should_Require_Owner_Or_Manager()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var otherRequesterId = await IdentityAccessSeed.SeedUserAsync(db, "Other", "other@x.com", "Requester");
            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db, "Falha", "Sem acesso", TicketStatus.Novo, TicketPriority.Media, requesterId, categoryId);

            var req = new UpdateTicketRequest { Title = "Título novo" };

            var http1 = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(otherRequesterId);

            var resp1 = await client.SendAsync(http1);
            Assert.Equal(HttpStatusCode.Forbidden, resp1.StatusCode);

            var http2 = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(managerId);

            var resp2 = await client.SendAsync(http2);
            Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);
        }

        [Fact]
        public async Task Update_Should_Return_404_When_Ticket_Not_Found()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), "/api/tickets/999999")
            {
                Content = JsonContent.Create(new UpdateTicketRequest { Title = "Novo" })
            }.WithUserId(managerId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task Update_Should_Block_Inactive_Tickets()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db, "Falha", "Sem acesso", TicketStatus.Fechado, TicketPriority.Media, requesterId, categoryId);

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}")
            {
                Content = JsonContent.Create(new UpdateTicketRequest { Title = "Novo" })
            }.WithUserId(requesterId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Update_Should_Return_400_When_No_Changes_Detected()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db, "Falha", "Sem acesso", TicketStatus.Novo, TicketPriority.Media, requesterId, categoryId);

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}")
            {
                Content = JsonContent.Create(new UpdateTicketRequest())
            }.WithUserId(requesterId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Update_Should_Validate_Priority_And_Category()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db, "Falha", "Sem acesso", TicketStatus.Novo, TicketPriority.Media, requesterId, categoryId);

            var badPriority = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}")
            {
                Content = JsonContent.Create(new UpdateTicketRequest { Priority = "Urgente" })
            }.WithUserId(requesterId);

            var respBadPriority = await client.SendAsync(badPriority);
            Assert.Equal(HttpStatusCode.BadRequest, respBadPriority.StatusCode);

            var badCategory = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}")
            {
                Content = JsonContent.Create(new UpdateTicketRequest { CategoryId = 999999 })
            }.WithUserId(requesterId);

            var respBadCategory = await client.SendAsync(badCategory);
            Assert.Equal(HttpStatusCode.BadRequest, respBadCategory.StatusCode);
        }

        [Fact]
        public async Task Update_Should_Update_Title_Description_Priority_And_Category_When_Valid()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var cat1 = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");
            var cat2 = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Software");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db, "Falha", "Sem acesso", TicketStatus.Novo, TicketPriority.Media, requesterId, cat1);

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}")
            {
                Content = JsonContent.Create(new UpdateTicketRequest
                {
                    Title = "  Falha atualizada  ",
                    Description = "  Descrição nova  ",
                    Priority = TicketPriority.Alta,
                    CategoryId = cat2
                })
            }.WithUserId(requesterId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var dto = await resp.Content.ReadFromJsonAsync<TicketResponse>();
            Assert.NotNull(dto);
            Assert.Equal(ticketId, dto!.Id);
            Assert.Equal("Falha atualizada", dto.Title);
            Assert.Equal("Descrição nova", dto.Description);
            Assert.Equal(TicketPriority.Alta, dto.Priority);
            Assert.Equal(cat2, dto.CategoryId);
        }
    }
}