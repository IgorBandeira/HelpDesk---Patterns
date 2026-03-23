using FluentAssertions;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.IntegrationTests.Attachments.Fixtures;
using HelpDesk.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace HelpDesk.IntegrationTests.Attachments
{
    public class Attachments_List_Tests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public Attachments_List_Tests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task List_Should_Return_Attachments_Ordered_By_Id_Desc()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = await AttachmentsSeed.SeedUserAsync(db, "Igor", "list@test.com");
            var category = await AttachmentsSeed.SeedCategoryAsync(db);
            var ticket = await AttachmentsSeed.SeedTicketAsync(db, user.Id, category.Id);

            var a1 = await AttachmentsSeed.SeedAttachmentAsync(db, ticket.Id, user.Id, "a.pdf");
            var a2 = await AttachmentsSeed.SeedAttachmentAsync(db, ticket.Id, user.Id, "b.pdf");

            var response = await client.GetAsync($"/api/tickets/{ticket.Id}/attachments");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var items = await response.Content.ReadFromJsonAsync<List<AttachmentItemResponseContract>>();
            items.Should().NotBeNull();
            items!.Count.Should().Be(2);
            items[0].Id.Should().BeGreaterThan(items[1].Id);
        }

        [Fact]
        public async Task List_Should_Return_NotFound_When_Ticket_Does_Not_Exist()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/tickets/999/attachments");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private sealed class AttachmentItemResponseContract
        {
            public int Id { get; set; }
            public int TicketId { get; set; }
            public string FileName { get; set; } = string.Empty;
            public string UploadedByName { get; set; } = string.Empty;
        }
    }
}