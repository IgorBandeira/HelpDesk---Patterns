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
    public class Tickets_GetById_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public Tickets_GetById_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task GetById_Should_Return_401_When_User_Is_Not_Informed()
        {
            var client = _factory.CreateClient();

            var resp = await client.GetAsync("/api/tickets/999999");

            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact]
        public async Task GetById_Should_Return_401_When_User_Is_Invalid()
        {
            var client = _factory.CreateClient();

            var req = new HttpRequestMessage(HttpMethod.Get, "/api/tickets/999999")
                .WithUserId(999999);

            var resp = await client.SendAsync(req);

            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact]
        public async Task GetById_Should_Return_404_When_Ticket_Not_Found()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");

            var req = new HttpRequestMessage(HttpMethod.Get, "/api/tickets/999999")
                .WithUserId(requesterId);

            var resp = await client.SendAsync(req);

            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task GetById_Should_Return_Ticket_When_Found()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db,
                "Falha no login",
                "Usuário sem acesso",
                TicketStatus.Novo,
                TicketPriority.Media,
                requesterId,
                categoryId);

            var req = new HttpRequestMessage(HttpMethod.Get, $"/api/tickets/{ticketId}")
                .WithUserId(requesterId);

            var resp = await client.SendAsync(req);

            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var dto = await resp.Content.ReadFromJsonAsync<TicketDetailsResponse>();
            Assert.NotNull(dto);
            Assert.Equal(ticketId, dto!.Id);
            Assert.Equal("Falha no login", dto.Title);
            Assert.Equal("Usuário sem acesso", dto.Description);
            Assert.Equal(TicketStatus.Novo, dto.Status);
            Assert.Equal(TicketPriority.Media, dto.Priority);

            Assert.NotNull(dto.Requester);
            Assert.Equal(requesterId, dto.Requester.Id);

            Assert.NotNull(dto.Category);
            Assert.Equal(categoryId, dto.Category.Id);
        }

        [Fact]
        public async Task GetById_Should_Allow_Manager_To_View_Ticket()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);
            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db,
                "Erro no sistema",
                "Tela azul",
                TicketStatus.EmAnalise,
                TicketPriority.Alta,
                requesterId,
                categoryId);

            var req = new HttpRequestMessage(HttpMethod.Get, $"/api/tickets/{ticketId}")
                .WithUserId(managerId);

            var resp = await client.SendAsync(req);

            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }
    }
}