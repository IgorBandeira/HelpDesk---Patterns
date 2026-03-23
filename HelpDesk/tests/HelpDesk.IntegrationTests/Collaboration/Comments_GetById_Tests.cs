using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Collaboration.Fixtures;
using HelpDesk.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace HelpDesk.IntegrationTests.Collaboration
{
    public class Comments_GetById_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public Comments_GetById_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task GetById_Should_Return_Ok_When_Public_Comment_Exists()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var requester = await CollaborationSeed.SeedUserAsync(db, "Req", "req_get1@x.com", "Requester");
            var ticket = await CollaborationSeed.SeedTicketAsync(db, "Ticket", "Desc", "Aberto", requester.Id);
            var comment = await CollaborationSeed.SeedCommentAsync(db, ticket.Id, requester.Id, "Público", "Comentário público");

            var http = new HttpRequestMessage(HttpMethod.Get, $"/api/tickets/{ticket.Id}/comments/{comment.Id}");
            http.Headers.Add("userId", requester.Id.ToString());

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_Should_Return_Ok_When_Internal_Comment_And_User_Is_Participant()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var requester = await CollaborationSeed.SeedUserAsync(db, "Req", "req_get2@x.com", "Requester");
            var ticket = await CollaborationSeed.SeedTicketAsync(db, "Ticket", "Desc", "Aberto", requester.Id);
            var comment = await CollaborationSeed.SeedCommentAsync(db, ticket.Id, requester.Id, "Interno", "Comentário interno");

            var http = new HttpRequestMessage(HttpMethod.Get, $"/api/tickets/{ticket.Id}/comments/{comment.Id}");
            http.Headers.Add("userId", requester.Id.ToString());

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_Should_Return_NotFound_When_Internal_Comment_And_User_Is_Not_Participant()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var requester = await CollaborationSeed.SeedUserAsync(db, "Req", "req_get3@x.com", "Requester");
            var outsider = await CollaborationSeed.SeedUserAsync(db, "Out", "out_get3@x.com", "Requester");
            var ticket = await CollaborationSeed.SeedTicketAsync(db, "Ticket", "Desc", "Aberto", requester.Id);
            var comment = await CollaborationSeed.SeedCommentAsync(db, ticket.Id, requester.Id, "Interno", "Segredo");

            var http = new HttpRequestMessage(HttpMethod.Get, $"/api/tickets/{ticket.Id}/comments/{comment.Id}");
            http.Headers.Add("userId", outsider.Id.ToString());

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetById_Should_Return_NotFound_When_Comment_Does_Not_Exist()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var requester = await CollaborationSeed.SeedUserAsync(db, "Req", "req_get4@x.com", "Requester");
            var ticket = await CollaborationSeed.SeedTicketAsync(db, "Ticket", "Desc", "Aberto", requester.Id);

            var http = new HttpRequestMessage(HttpMethod.Get, $"/api/tickets/{ticket.Id}/comments/999999");
            http.Headers.Add("userId", requester.Id.ToString());

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}