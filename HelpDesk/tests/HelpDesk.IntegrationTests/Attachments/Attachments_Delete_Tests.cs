using FluentAssertions;
using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Attachments.Fixtures;
using HelpDesk.IntegrationTests.Fakes;
using HelpDesk.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace HelpDesk.IntegrationTests.Attachments
{
    public class Attachments_Delete_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public Attachments_Delete_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task Delete_Should_Remove_From_Database_And_Call_Storage()
        {
            _factory.FileStorageFake.Reset();

            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = await AttachmentsSeed.SeedUserAsync(db, "Igor", "del@test.com");
            var category = await AttachmentsSeed.SeedCategoryAsync(db);
            var ticket = await AttachmentsSeed.SeedTicketAsync(db, user.Id, category.Id);
            var attachment = await AttachmentsSeed.SeedAttachmentAsync(db, ticket.Id, user.Id, "remover.pdf");

            var request = new HttpRequestMessage(
                HttpMethod.Delete,
                $"/api/tickets/{ticket.Id}/attachments/{attachment.Id}");

            request.Headers.Add("userId", user.Id.ToString());

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var assertScope = _factory.Services.CreateScope();
            var assertDb = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();

            assertDb.Attachments.Any(x => x.Id == attachment.Id).Should().BeFalse();
            _factory.FileStorageFake.DeletedKeys.Should().Contain(attachment.StorageKey);
        }

        [Fact]
        public async Task Delete_Should_Return_NotFound_When_Ticket_Does_Not_Exist()
        {
            var client = _factory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Delete, "/api/tickets/999/attachments/1");
            request.Headers.Add("userId", "1");

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_Should_Return_NotFound_When_Attachment_Does_Not_Exist()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = await AttachmentsSeed.SeedUserAsync(db, "Igor", "del2@test.com");
            var category = await AttachmentsSeed.SeedCategoryAsync(db);
            var ticket = await AttachmentsSeed.SeedTicketAsync(db, user.Id, category.Id);

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/tickets/{ticket.Id}/attachments/999");
            request.Headers.Add("userId", user.Id.ToString());

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_Should_Return_BadRequest_When_Ticket_Is_Closed()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = await AttachmentsSeed.SeedUserAsync(db, "Igor", "del3@test.com");
            var category = await AttachmentsSeed.SeedCategoryAsync(db);
            var ticket = await AttachmentsSeed.SeedTicketAsync(db, user.Id, category.Id, "Fechado");
            var attachment = await AttachmentsSeed.SeedAttachmentAsync(db, ticket.Id, user.Id);

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/tickets/{ticket.Id}/attachments/{attachment.Id}");
            request.Headers.Add("userId", user.Id.ToString());

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Delete_Should_Return_Forbidden_When_User_Is_Not_Author()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var author = await AttachmentsSeed.SeedUserAsync(db, "Autor", "autor@test.com");
            var other = await AttachmentsSeed.SeedUserAsync(db, "Outro", "other@test.com");
            var category = await AttachmentsSeed.SeedCategoryAsync(db);
            var ticket = await AttachmentsSeed.SeedTicketAsync(db, author.Id, category.Id);
            var attachment = await AttachmentsSeed.SeedAttachmentAsync(db, ticket.Id, author.Id);

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/tickets/{ticket.Id}/attachments/{attachment.Id}");
            request.Headers.Add("userId", other.Id.ToString());

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}