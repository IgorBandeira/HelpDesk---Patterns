using FluentAssertions;
using HelpDesk.Api.Contracts.IdentityAccess.Responses;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Ticketing.Models;
using HelpDesk.IntegrationTests.Fixtures;
using HelpDesk.IntegrationTests.IdentityAccess.Seed;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace HelpDesk.IntegrationTests.IdentityAccess
{
    public class Users_GetById_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public Users_GetById_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task GetById_Should_Return_404_When_User_Not_Found()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/api/users/999999");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task GetById_Should_Return_User_With_Requested_And_Assigned_Tickets()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var agentId = await IdentityAccessSeed.SeedUserAsync(db, "Agent", "agent@x.com", "Agent");

            db.Tickets.AddRange(
                new TicketEntity
                {
                    Title = "T-Req",
                    Status = "Novo",
                    PriorityLevel = "Média",
                    RequesterId = requesterId,
                    AssigneeId = null
                },
                new TicketEntity
                {
                    Title = "T-Ass",
                    Status = "Em Andamento",
                    PriorityLevel = "Alta",
                    RequesterId = requesterId,
                    AssigneeId = agentId
                }
            );
            await db.SaveChangesAsync();

            var resp = await client.GetAsync($"/api/users/{requesterId}");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var dto = await resp.Content.ReadFromJsonAsync<UserWithTicketsResponse>();
            Assert.NotNull(dto);

            dto!.Id.Should().Be(requesterId);
            dto.RequestedTickets.Should().NotBeEmpty();
            dto.AssignedTickets.Should().BeEmpty();

            var resp2 = await client.GetAsync($"/api/users/{agentId}");
            Assert.Equal(HttpStatusCode.OK, resp2.StatusCode);

            var dto2 = await resp2.Content.ReadFromJsonAsync<UserWithTicketsResponse>();
            Assert.NotNull(dto2);
            dto2!.AssignedTickets.Should().NotBeEmpty();
        }
    }
}
