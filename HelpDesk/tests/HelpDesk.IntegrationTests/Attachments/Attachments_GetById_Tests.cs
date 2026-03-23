using FluentAssertions;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Attachments.Fixtures;
using HelpDesk.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace HelpDesk.IntegrationTests.Attachments
{
    public class Attachments_GetById_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public Attachments_GetById_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task GetById_Should_Return_Attachment_When_It_Exists()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = await AttachmentsSeed.SeedUserAsync(db, "Igor", "get@test.com");
            var category = await AttachmentsSeed.SeedCategoryAsync(db);
            var ticket = await AttachmentsSeed.SeedTicketAsync(db, user.Id, category.Id);
            var attachment = await AttachmentsSeed.SeedAttachmentAsync(db, ticket.Id, user.Id, "doc.pdf");

            var response = await client.GetAsync($"/api/tickets/{ticket.Id}/attachments/{attachment.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var item = await response.Content.ReadFromJsonAsync<AttachmentItemResponseContract>();
            item.Should().NotBeNull();
            item!.Id.Should().Be(attachment.Id);
            item.FileName.Should().Be("doc.pdf");
        }

        [Fact]
        public async Task GetById_Should_Return_NotFound_When_Attachment_Does_Not_Exist()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/tickets/1/attachments/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private sealed class AttachmentItemResponseContract
        {
            public int Id { get; set; }
            public string FileName { get; set; } = string.Empty;
        }
    }
}