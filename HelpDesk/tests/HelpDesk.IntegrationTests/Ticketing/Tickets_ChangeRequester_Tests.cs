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
    public class Tickets_ChangeRequester_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public Tickets_ChangeRequester_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task ChangeRequester_Should_Require_Owner_Or_Manager()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var otherRequesterId = await IdentityAccessSeed.SeedUserAsync(db, "Other", "other@x.com", "Requester");
            var newRequesterId = await IdentityAccessSeed.SeedUserAsync(db, "NewReq", "newreq@x.com", "Requester");
            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db, "Falha", "Sem acesso", TicketStatus.Novo, TicketPriority.Media, requesterId, categoryId);

            var req = new ChangeRequesterRequest { RequesterId = newRequesterId };

            var http1 = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}/requester")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(otherRequesterId);

            var resp1 = await client.SendAsync(http1);
            Assert.Equal(HttpStatusCode.Forbidden, resp1.StatusCode);

            var http2 = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}/requester")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(managerId);

            var resp2 = await client.SendAsync(http2);
            Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);
        }

        [Fact]
        public async Task ChangeRequester_Should_Return_404_When_Ticket_Not_Found()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var newRequesterId = await IdentityAccessSeed.SeedUserAsync(db, "NewReq", "newreq@x.com", "Requester");

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), "/api/tickets/999999/requester")
            {
                Content = JsonContent.Create(new ChangeRequesterRequest { RequesterId = newRequesterId })
            }.WithUserId(requesterId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task ChangeRequester_Should_Block_Inactive_Tickets()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var newRequesterId = await IdentityAccessSeed.SeedUserAsync(db, "NewReq", "newreq@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db, "Falha", "Sem acesso", TicketStatus.Fechado, TicketPriority.Media, requesterId, categoryId);

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}/requester")
            {
                Content = JsonContent.Create(new ChangeRequesterRequest { RequesterId = newRequesterId })
            }.WithUserId(requesterId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task ChangeRequester_Should_Return_400_When_User_Not_Found()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db, "Falha", "Sem acesso", TicketStatus.Novo, TicketPriority.Media, requesterId, categoryId);

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}/requester")
            {
                Content = JsonContent.Create(new ChangeRequesterRequest { RequesterId = 999999 })
            }.WithUserId(requesterId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task ChangeRequester_Should_Return_400_When_User_Is_Not_Requester()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var agentId = await IdentityAccessSeed.SeedUserAsync(db, "Agent", "agent@x.com", "Agent");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db, "Falha", "Sem acesso", TicketStatus.Novo, TicketPriority.Media, requesterId, categoryId);

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}/requester")
            {
                Content = JsonContent.Create(new ChangeRequesterRequest { RequesterId = agentId })
            }.WithUserId(requesterId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task ChangeRequester_Should_Update_Requester_When_Valid()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var newRequesterId = await IdentityAccessSeed.SeedUserAsync(db, "NewReq", "newreq@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db, "Falha", "Sem acesso", TicketStatus.Novo, TicketPriority.Media, requesterId, categoryId);

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}/requester")
            {
                Content = JsonContent.Create(new ChangeRequesterRequest { RequesterId = newRequesterId })
            }.WithUserId(requesterId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var dto = await resp.Content.ReadFromJsonAsync<RequesterResponse>();
            Assert.NotNull(dto);
            Assert.Equal(ticketId, dto!.TicketId);
            Assert.Equal(TicketStatus.Novo, dto.Status);
            Assert.Equal(newRequesterId, dto.RequesterId);
            Assert.Equal("NewReq", dto.RequesterName);
        }
    }
}