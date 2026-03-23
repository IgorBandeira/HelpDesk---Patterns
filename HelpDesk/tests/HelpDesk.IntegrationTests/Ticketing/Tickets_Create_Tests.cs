using HelpDesk.Api.Contracts.Ticketing.Requests;
using HelpDesk.Api.Contracts.Ticketing.Responses;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Fixtures;
using HelpDesk.IntegrationTests.IdentityAccess.Seed;
using HelpDesk.IntegrationTests.ServiceCatalog.Seed;
using HelpDesk.IntegrationTests.Utilities.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace HelpDesk.IntegrationTests.Ticketing
{
    public class Tickets_Create_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public Tickets_Create_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task Create_Should_Require_Requester_Or_Manager()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var agentId = await IdentityAccessSeed.SeedUserAsync(db, "Agent", "agent@x.com", "Agent");
            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var managerId = await IdentityAccessSeed.SeedManagerAsync(db);
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var req = new CreateTicketRequest
            {
                Title = "Falha",
                Description = "Sem acesso",
                Priority = "Média",
                CategoryId = categoryId
            };

            var http1 = new HttpRequestMessage(HttpMethod.Post, "/api/tickets")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(agentId);

            var resp1 = await client.SendAsync(http1);
            Assert.Equal(HttpStatusCode.Forbidden, resp1.StatusCode);

            var http2 = new HttpRequestMessage(HttpMethod.Post, "/api/tickets")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(requesterId);

            var resp2 = await client.SendAsync(http2);
            Assert.Equal(HttpStatusCode.Created, resp2.StatusCode);

            var http3 = new HttpRequestMessage(HttpMethod.Post, "/api/tickets")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(managerId);

            var resp3 = await client.SendAsync(http3);
            Assert.Equal(HttpStatusCode.Created, resp3.StatusCode);
        }

        [Fact]
        public async Task Create_Should_Return_401_When_User_Is_Invalid()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var req = new CreateTicketRequest
            {
                Title = "Falha",
                Description = "Sem acesso",
                Priority = TicketPriority.Media,
                CategoryId = categoryId
            };

            var http = new HttpRequestMessage(HttpMethod.Post, "/api/tickets")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(999999);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        }

        [Fact]
        public async Task Create_Should_Return_400_When_Title_Is_Missing()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var req = new CreateTicketRequest
            {
                Title = "   ",
                Description = "Sem acesso",
                Priority = TicketPriority.Media,
                CategoryId = categoryId
            };

            var http = new HttpRequestMessage(HttpMethod.Post, "/api/tickets")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(requesterId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Create_Should_Return_400_When_Description_Is_Missing()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var req = new CreateTicketRequest
            {
                Title = "Falha",
                Description = "   ",
                Priority = TicketPriority.Media,
                CategoryId = categoryId
            };

            var http = new HttpRequestMessage(HttpMethod.Post, "/api/tickets")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(requesterId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Create_Should_Return_400_When_Priority_Is_Invalid()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var req = new CreateTicketRequest
            {
                Title = "Falha",
                Description = "Sem acesso",
                Priority = "Urgente",
                CategoryId = categoryId
            };

            var http = new HttpRequestMessage(HttpMethod.Post, "/api/tickets")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(requesterId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Create_Should_Return_400_When_Category_Does_Not_Exist()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");

            var req = new CreateTicketRequest
            {
                Title = "Falha",
                Description = "Sem acesso",
                Priority = TicketPriority.Media,
                CategoryId = 999999
            };

            var http = new HttpRequestMessage(HttpMethod.Post, "/api/tickets")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(requesterId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task Create_Should_Create_Ticket_When_Request_Is_Valid()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbReset.ResetAsync(db);

            var requesterId = await IdentityAccessSeed.SeedUserAsync(db, "Req", "req@x.com", "Requester");
            var categoryId = await ServiceCatalogSeed.SeedRootCategoryAsync(db, "Hardware");

            var req = new CreateTicketRequest
            {
                Title = "  Falha no login  ",
                Description = "  Usuário sem acesso ao sistema  ",
                Priority = TicketPriority.Alta,
                CategoryId = categoryId
            };

            var http = new HttpRequestMessage(HttpMethod.Post, "/api/tickets")
            {
                Content = JsonContent.Create(req)
            }.WithUserId(requesterId);

            var resp = await client.SendAsync(http);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

            var dto = await resp.Content.ReadFromJsonAsync<TicketResponse>();
            Assert.NotNull(dto);
            Assert.Equal("Falha no login", dto!.Title);
            Assert.Equal("Usuário sem acesso ao sistema", dto.Description);
            Assert.Equal(TicketStatus.Novo, dto.Status);
            Assert.Equal(TicketPriority.Alta, dto.Priority);
            Assert.Equal(requesterId, dto.RequesterId);
            Assert.Equal(categoryId, dto.CategoryId);
            Assert.True(dto.Id > 0);
        }
    }
}