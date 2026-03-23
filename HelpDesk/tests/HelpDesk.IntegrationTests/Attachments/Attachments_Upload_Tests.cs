using FluentAssertions;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Attachments.Fixtures;
using HelpDesk.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;

namespace HelpDesk.IntegrationTests.Attachments
{
    public class Attachments_Upload_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public Attachments_Upload_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task Upload_Should_Create_Attachment_When_Data_Is_Valid()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = await AttachmentsSeed.SeedUserAsync(db, "Igor", "igor@test.com");
            var category = await AttachmentsSeed.SeedCategoryAsync(db);
            var ticket = await AttachmentsSeed.SeedTicketAsync(db, user.Id, category.Id, "Aberto");

            using var form = new MultipartFormDataContent();
            using var content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3, 4 }));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            form.Add(content, "file", "teste.pdf");

            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/tickets/{ticket.Id}/attachments");
            request.Headers.Add("userId", user.Id.ToString());
            request.Content = form;

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var saved = db.Attachments.Single();
            saved.TicketId.Should().Be(ticket.Id);
            saved.UploadedById.Should().Be(user.Id);
            saved.FileName.Should().Be("teste.pdf");
        }

        [Fact]
        public async Task Upload_Should_Return_NotFound_When_Ticket_Does_Not_Exist()
        {
            var client = _factory.CreateClient();

            using var form = new MultipartFormDataContent();
            using var content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3 }));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            form.Add(content, "file", "teste.pdf");

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/tickets/999/attachments");
            request.Headers.Add("userId", "1");
            request.Content = form;

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Upload_Should_Return_BadRequest_When_Ticket_Is_Closed()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = await AttachmentsSeed.SeedUserAsync(db, "Igor", "igor2@test.com");
            var category = await AttachmentsSeed.SeedCategoryAsync(db);
            var ticket = await AttachmentsSeed.SeedTicketAsync(db, user.Id, category.Id, "Fechado");

            using var form = new MultipartFormDataContent();
            using var content = new StreamContent(new MemoryStream(new byte[] { 1, 2, 3 }));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            form.Add(content, "file", "teste.pdf");

            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/tickets/{ticket.Id}/attachments");
            request.Headers.Add("userId", user.Id.ToString());
            request.Content = form;

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Upload_Should_Return_BadRequest_When_User_Is_Invalid()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var owner = await AttachmentsSeed.SeedUserAsync(db, "Igor", "igor3@test.com");
            var category = await AttachmentsSeed.SeedCategoryAsync(db);
            var ticket = await AttachmentsSeed.SeedTicketAsync(db, owner.Id, category.Id, "Aberto");

            using var form = new MultipartFormDataContent();
            using var content = new StreamContent(new MemoryStream(new byte[] { 1 }));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            form.Add(content, "file", "teste.pdf");

            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/tickets/{ticket.Id}/attachments");
            request.Headers.Add("userId", "999");
            request.Content = form;

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Upload_Should_Return_BadRequest_When_Extension_Is_Blocked()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = await AttachmentsSeed.SeedUserAsync(db, "Igor", "igor4@test.com");
            var category = await AttachmentsSeed.SeedCategoryAsync(db);
            var ticket = await AttachmentsSeed.SeedTicketAsync(db, user.Id, category.Id, "Aberto");

            using var form = new MultipartFormDataContent();
            using var content = new StreamContent(new MemoryStream(new byte[] { 1, 2 }));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            form.Add(content, "file", "virus.exe");

            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/tickets/{ticket.Id}/attachments");
            request.Headers.Add("userId", user.Id.ToString());
            request.Content = form;

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}