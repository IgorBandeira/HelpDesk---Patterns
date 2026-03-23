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
    public class Tickets_Cancel_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public Tickets_Cancel_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task Cancel_Should_Require_Owner_Or_Manager()
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

            var req = new CancelTicketRequest { Reason = "Desistiu" };

            var http1 = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}/cancel")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(otherRequesterId);

            var resp1 = await client.SendAsync(http1);
            Assert.Equal(HttpStatusCode.Forbidden, resp1.StatusCode);

            var http2 = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}/cancel")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(managerId);

            var resp2 = await client.SendAsync(http2);
            Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);
        }

        [Fact]
        public async Task Cancel_Should_Return_404_When_Ticket_Not_Found()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), "/api/tickets/999999/cancel")
            {
                Content = JsonContent.Create(new CancelTicketRequest { Reason = "Teste" })
            }.WithUserId(managerId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task Cancel_Should_Return_400_When_Reason_Is_Missing()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db, "Falha", "Sem acesso", TicketStatus.Novo, TicketPriority.Media, requesterId, categoryId);

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}/cancel")
            {
                Content = JsonContent.Create(new CancelTicketRequest { Reason = " " })
            }.WithUserId(requesterId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Cancel_Should_Return_400_When_Status_Is_Not_Novo_Or_EmAnalise()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db, "Falha", "Sem acesso", TicketStatus.Resolvido, TicketPriority.Media, requesterId, categoryId);

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}/cancel")
            {
                Content = JsonContent.Create(new CancelTicketRequest { Reason = "Não precisa mais" })
            }.WithUserId(requesterId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Cancel_Should_Move_To_Cancelado_When_Valid()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db, "Falha", "Sem acesso", TicketStatus.EmAnalise, TicketPriority.Media, requesterId, categoryId);

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}/cancel")
            {
                Content = JsonContent.Create(new CancelTicketRequest { Reason = "Não precisa mais" })
            }.WithUserId(requesterId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var dto = await resp.Content.ReadFromJsonAsync<CancelTicketResponse>();
            Assert.NotNull(dto);
            Assert.Equal(ticketId, dto!.TicketId);
            Assert.Equal(TicketStatus.EmAnalise, dto.PreviousStatus);
            Assert.Equal(TicketStatus.Cancelado, dto.NewStatus);
            Assert.Equal(requesterId, dto.ActorUserId);
            Assert.Equal("Não precisa mais", dto.Reason);
        }
    }
}