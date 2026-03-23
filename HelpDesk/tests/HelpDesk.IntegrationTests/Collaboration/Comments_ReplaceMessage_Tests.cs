using HelpDesk.Api.Contracts.Collaboration.Requests;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Collaboration.Fixtures;
using HelpDesk.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace HelpDesk.IntegrationTests.Collaboration
{
    public class Comments_ReplaceMessage_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public Comments_ReplaceMessage_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task Replace_Should_Return_Ok_When_Author_Updates_Message()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var requester = await CollaborationSeed.SeedUserAsync(db, "Req", "req_replace1@x.com", "Requester");
            var ticket = await CollaborationSeed.SeedTicketAsync(db, "Ticket", "Desc", "Aberto", requester.Id);
            var comment = await CollaborationSeed.SeedCommentAsync(db, ticket.Id, requester.Id, "Público", "Texto antigo");

            var request = new ReplaceCommentMessageRequest
            {
                Message = "Texto novo"
            };

            var http = new HttpRequestMessage(HttpMethod.Put, $"/api/tickets/{ticket.Id}/comments/{comment.Id}");
            http.Headers.Add("userId", requester.Id.ToString());
            http.Content = JsonContent.Create(request);

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Replace_Should_Return_Forbidden_When_User_Is_Not_Author()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var author = await CollaborationSeed.SeedUserAsync(db, "Author", "author_replace2@x.com", "Requester");
            var outsider = await CollaborationSeed.SeedUserAsync(db, "Out", "out_replace2@x.com", "Requester");
            var ticket = await CollaborationSeed.SeedTicketAsync(db, "Ticket", "Desc", "Aberto", author.Id);
            var comment = await CollaborationSeed.SeedCommentAsync(db, ticket.Id, author.Id, "Público", "Texto antigo");

            var request = new ReplaceCommentMessageRequest
            {
                Message = "Texto novo"
            };

            var http = new HttpRequestMessage(HttpMethod.Put, $"/api/tickets/{ticket.Id}/comments/{comment.Id}");
            http.Headers.Add("userId", outsider.Id.ToString());
            http.Content = JsonContent.Create(request);

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Replace_Should_Return_BadRequest_When_Message_Is_Equal()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var author = await CollaborationSeed.SeedUserAsync(db, "Author", "author_replace3@x.com", "Requester");
            var ticket = await CollaborationSeed.SeedTicketAsync(db, "Ticket", "Desc", "Aberto", author.Id);
            var comment = await CollaborationSeed.SeedCommentAsync(db, ticket.Id, author.Id, "Público", "Mesmo texto");

            var request = new ReplaceCommentMessageRequest
            {
                Message = "Mesmo texto"
            };

            var http = new HttpRequestMessage(HttpMethod.Put, $"/api/tickets/{ticket.Id}/comments/{comment.Id}");
            http.Headers.Add("userId", author.Id.ToString());
            http.Content = JsonContent.Create(request);

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Replace_Should_Return_BadRequest_When_Ticket_Is_Inactive()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var author = await CollaborationSeed.SeedUserAsync(db, "Author", "author_replace4@x.com", "Requester");
            var ticket = await CollaborationSeed.SeedTicketAsync(db, "Ticket fechado", "Desc", "Cancelado", author.Id);
            var comment = await CollaborationSeed.SeedCommentAsync(db, ticket.Id, author.Id, "Público", "Texto antigo");

            var request = new ReplaceCommentMessageRequest
            {
                Message = "Texto novo"
            };

            var http = new HttpRequestMessage(HttpMethod.Put, $"/api/tickets/{ticket.Id}/comments/{comment.Id}");
            http.Headers.Add("userId", author.Id.ToString());
            http.Content = JsonContent.Create(request);

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Replace_Should_Return_NotFound_When_Comment_Does_Not_Exist()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var author = await CollaborationSeed.SeedUserAsync(db, "Author", "author_replace5@x.com", "Requester");
            var ticket = await CollaborationSeed.SeedTicketAsync(db, "Ticket", "Desc", "Aberto", author.Id);

            var request = new ReplaceCommentMessageRequest
            {
                Message = "Texto novo"
            };

            var http = new HttpRequestMessage(HttpMethod.Put, $"/api/tickets/{ticket.Id}/comments/999999");
            http.Headers.Add("userId", author.Id.ToString());
            http.Content = JsonContent.Create(request);

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}