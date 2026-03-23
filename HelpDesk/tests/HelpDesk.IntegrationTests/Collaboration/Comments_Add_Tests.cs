using HelpDesk.Api.Contracts.Collaboration.Requests;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Collaboration.Fixtures;
using HelpDesk.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace HelpDesk.IntegrationTests.Collaboration
{
    public class Comments_Add_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public Comments_Add_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task Add_Public_Should_Return_Created_When_Requester_Is_Valid()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var requester = await CollaborationSeed.SeedUserAsync(db, "Requester", "requester1@x.com", "Requester");
            var category = await CollaborationSeed.SeedCategoryAsync(db, "Infra");

            var ticket = await CollaborationSeed.SeedTicketAsync(
                db,
                "Ticket 1",
                "Desc",
                "Aberto",
                requester.Id,
                categoryId: category.Id);

            var request = new AddCommentRequest
            {
                Message = "Comentário público",
                Visibility = "Público"
            };

            var http = new HttpRequestMessage(HttpMethod.Post, $"/api/tickets/{ticket.Id}/comments");
            http.Headers.Add("userId", requester.Id.ToString());
            http.Content = JsonContent.Create(request);

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Add_Internal_Should_Return_Created_When_Requester_Is_Participant()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var requester = await CollaborationSeed.SeedUserAsync(db, "Requester", "requester2@x.com", "Requester");
            var category = await CollaborationSeed.SeedCategoryAsync(db, "Infra");

            var ticket = await CollaborationSeed.SeedTicketAsync(
                db,
                "Ticket interno",
                "Desc",
                "Aberto",
                requester.Id,
                categoryId: category.Id);

            var request = new AddCommentRequest
            {
                Message = "Comentário interno",
                Visibility = "Interno"
            };

            var http = new HttpRequestMessage(HttpMethod.Post, $"/api/tickets/{ticket.Id}/comments");
            http.Headers.Add("userId", requester.Id.ToString());
            http.Content = JsonContent.Create(request);

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Add_Internal_Should_Return_Forbidden_When_User_Is_Not_Participant()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var requester = await CollaborationSeed.SeedUserAsync(db, "Requester", "requester3@x.com", "Requester");
            var outsider = await CollaborationSeed.SeedUserAsync(db, "Outsider", "outsider1@x.com", "Requester");
            var category = await CollaborationSeed.SeedCategoryAsync(db, "Infra");

            var ticket = await CollaborationSeed.SeedTicketAsync(
                db,
                "Ticket",
                "Desc",
                "Aberto",
                requester.Id,
                categoryId: category.Id);

            var request = new AddCommentRequest
            {
                Message = "Tentativa interna",
                Visibility = "Interno"
            };

            var http = new HttpRequestMessage(HttpMethod.Post, $"/api/tickets/{ticket.Id}/comments");
            http.Headers.Add("userId", outsider.Id.ToString());
            http.Content = JsonContent.Create(request);

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Add_Should_Return_BadRequest_When_Message_Is_Empty()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var requester = await CollaborationSeed.SeedUserAsync(db, "Requester", "requester4@x.com", "Requester");
            var category = await CollaborationSeed.SeedCategoryAsync(db, "Infra");

            var ticket = await CollaborationSeed.SeedTicketAsync(
                db,
                "Ticket",
                "Desc",
                "Aberto",
                requester.Id,
                categoryId: category.Id);

            var request = new AddCommentRequest
            {
                Message = "   ",
                Visibility = "Público"
            };

            var http = new HttpRequestMessage(HttpMethod.Post, $"/api/tickets/{ticket.Id}/comments");
            http.Headers.Add("userId", requester.Id.ToString());
            http.Content = JsonContent.Create(request);

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Add_Should_Return_BadRequest_When_Ticket_Is_Inactive()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var requester = await CollaborationSeed.SeedUserAsync(db, "Requester", "requester5@x.com", "Requester");
            var category = await CollaborationSeed.SeedCategoryAsync(db, "Infra");

            var ticket = await CollaborationSeed.SeedTicketAsync(
                db,
                "Ticket fechado",
                "Desc",
                "Fechado",
                requester.Id,
                categoryId: category.Id);

            var request = new AddCommentRequest
            {
                Message = "Comentário inválido",
                Visibility = "Público"
            };

            var http = new HttpRequestMessage(HttpMethod.Post, $"/api/tickets/{ticket.Id}/comments");
            http.Headers.Add("userId", requester.Id.ToString());
            http.Content = JsonContent.Create(request);

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Add_Should_Return_NotFound_When_Ticket_Does_Not_Exist()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var requester = await CollaborationSeed.SeedUserAsync(db, "Requester", "requester6@x.com", "Requester");

            var request = new AddCommentRequest
            {
                Message = "Comentário",
                Visibility = "Público"
            };

            var http = new HttpRequestMessage(HttpMethod.Post, "/api/tickets/999999/comments");
            http.Headers.Add("userId", requester.Id.ToString());
            http.Content = JsonContent.Create(request);

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}