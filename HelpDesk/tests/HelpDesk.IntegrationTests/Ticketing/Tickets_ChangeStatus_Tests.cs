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
    public class Tickets_ChangeStatus_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public Tickets_ChangeStatus_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task ChangeStatus_Should_Return_404_When_Ticket_Not_Found()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), "/api/tickets/999999/status")
            {
                Content = JsonContent.Create(new ChangeStatusRequest { NewStatus = TicketStatus.EmAndamento })
            }.WithUserId(managerId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task ChangeStatus_Should_Return_400_When_Transition_Is_Invalid()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db, "Falha", "Sem acesso", TicketStatus.Novo, TicketPriority.Media, requesterId, categoryId);

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}/status")
            {
                Content = JsonContent.Create(new ChangeStatusRequest { NewStatus = TicketStatus.Fechado })
            }.WithUserId(requesterId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task ChangeStatus_Should_Return_403_When_Agent_Is_Not_Assignee()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var assigneeId = await IdentityAccessSeed.SeedUserAsync(db, "Agent1", "agent1@x.com", "Agent");
            var otherAgentId = await IdentityAccessSeed.SeedUserAsync(db, "Agent2", "agent2@x.com", "Agent");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db, "Falha", "Sem acesso", TicketStatus.EmAnalise, TicketPriority.Media, requesterId, categoryId, assigneeId);

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}/status")
            {
                Content = JsonContent.Create(new ChangeStatusRequest { NewStatus = TicketStatus.EmAndamento })
            }.WithUserId(otherAgentId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
        }

        [Fact]
        public async Task ChangeStatus_Should_Allow_Assigned_Agent_To_Move_EmAnalise_To_EmAndamento()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var assigneeId = await IdentityAccessSeed.SeedUserAsync(db, "Agent", "agent@x.com", "Agent");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db, "Falha", "Sem acesso", TicketStatus.EmAnalise, TicketPriority.Media, requesterId, categoryId, assigneeId);

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}/status")
            {
                Content = JsonContent.Create(new ChangeStatusRequest { NewStatus = TicketStatus.EmAndamento })
            }.WithUserId(assigneeId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var dto = await resp.Content.ReadFromJsonAsync<ChangeStatusResponse>();
            Assert.NotNull(dto);
            Assert.Equal(ticketId, dto!.TicketId);
            Assert.Equal(TicketStatus.EmAnalise, dto.PreviousStatus);
            Assert.Equal(TicketStatus.EmAndamento, dto.NewStatus);
            Assert.Null(dto.ClosedAt);
        }

        [Fact]
        public async Task ChangeStatus_Should_Allow_Assigned_Agent_To_Move_EmAndamento_To_Resolvido()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var assigneeId = await IdentityAccessSeed.SeedUserAsync(db, "Agent", "agent@x.com", "Agent");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db, "Falha", "Sem acesso", TicketStatus.EmAndamento, TicketPriority.Media, requesterId, categoryId, assigneeId);

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}/status")
            {
                Content = JsonContent.Create(new ChangeStatusRequest { NewStatus = TicketStatus.Resolvido })
            }.WithUserId(assigneeId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        [Fact]
        public async Task ChangeStatus_Should_Allow_Requester_To_Move_Resolvido_To_Fechado()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var assigneeId = await IdentityAccessSeed.SeedUserAsync(db, "Agent", "agent@x.com", "Agent");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var ticketId = await TicketingSeed.SeedTicketAsync(
                db, "Falha", "Sem acesso", TicketStatus.Resolvido, TicketPriority.Media, requesterId, categoryId, assigneeId);

            var http = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/tickets/{ticketId}/status")
            {
                Content = JsonContent.Create(new ChangeStatusRequest { NewStatus = TicketStatus.Fechado })
            }.WithUserId(requesterId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var dto = await resp.Content.ReadFromJsonAsync<ChangeStatusResponse>();
            Assert.NotNull(dto);
            Assert.Equal(TicketStatus.Resolvido, dto!.PreviousStatus);
            Assert.Equal(TicketStatus.Fechado, dto.NewStatus);
            Assert.NotNull(dto.ClosedAt);
        }
    }
}