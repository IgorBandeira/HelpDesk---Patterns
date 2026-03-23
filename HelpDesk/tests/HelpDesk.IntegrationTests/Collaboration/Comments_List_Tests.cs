using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Collaboration.Fixtures;
using HelpDesk.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace HelpDesk.IntegrationTests.Collaboration
{
    public class Comments_List_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public Comments_List_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task List_Should_Return_All_When_User_Is_Participant()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var requester = await CollaborationSeed.SeedUserAsync(db, "Req", "req_list1@x.com", "Requester");
            var ticket = await CollaborationSeed.SeedTicketAsync(db, "Ticket", "Desc", "Aberto", requester.Id);

            await CollaborationSeed.SeedCommentAsync(db, ticket.Id, requester.Id, "Público", "Público");
            await CollaborationSeed.SeedCommentAsync(db, ticket.Id, requester.Id, "Interno", "Interno");

            var http = new HttpRequestMessage(HttpMethod.Get, $"/api/tickets/{ticket.Id}/comments");
            http.Headers.Add("userId", requester.Id.ToString());

            var response = await client.SendAsync(http);
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var doc = JsonDocument.Parse(body);
            Assert.Equal(2, doc.RootElement.GetArrayLength());
        }

        [Fact]
        public async Task List_Should_Return_Only_Public_When_User_Is_Not_Participant()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var requester = await CollaborationSeed.SeedUserAsync(db, "Req", "req_list2@x.com", "Requester");
            var outsider = await CollaborationSeed.SeedUserAsync(db, "Out", "out_list2@x.com", "Requester");
            var ticket = await CollaborationSeed.SeedTicketAsync(db, "Ticket", "Desc", "Aberto", requester.Id);

            await CollaborationSeed.SeedCommentAsync(db, ticket.Id, requester.Id, "Público", "Público");
            await CollaborationSeed.SeedCommentAsync(db, ticket.Id, requester.Id, "Interno", "Interno");

            var http = new HttpRequestMessage(HttpMethod.Get, $"/api/tickets/{ticket.Id}/comments");
            http.Headers.Add("userId", outsider.Id.ToString());

            var response = await client.SendAsync(http);
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var doc = JsonDocument.Parse(body);
            Assert.Single(doc.RootElement.EnumerateArray());
        }

        [Fact]
        public async Task List_Should_Return_NotFound_When_Ticket_Does_Not_Exist()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = await CollaborationSeed.SeedUserAsync(db, "Req", "req_list3@x.com", "Requester");

            var http = new HttpRequestMessage(HttpMethod.Get, "/api/tickets/999999/comments");
            http.Headers.Add("userId", user.Id.ToString());

            var response = await client.SendAsync(http);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}