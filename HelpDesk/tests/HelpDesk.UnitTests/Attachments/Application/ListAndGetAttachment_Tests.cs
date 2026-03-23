using FluentAssertions;
using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Application.Attachments.UseCases.GetAttachmentById;
using HelpDesk.Application.Attachments.UseCases.ListAttachments;
using HelpDesk.Application.Attachments.UseCases.UploadAttachment;
using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.UnitTests.Attachments.Fakes;
using HelpDesk.UnitTests.Shared.Fakes;

namespace HelpDesk.UnitTests.Attachments.Application
{
    public class ListAndGetAttachment_Tests
    {
        [Fact]
        public async Task List_Should_Return_404_When_Ticket_Not_Found()
        {
            var tickets = new InMemoryTicketReadPort();
            var users = new InMemoryUserReadPort();
            var repo = new InMemoryAttachmentRepository();

            var handler = new ListAttachmentsHandler(tickets, repo, users);

            var act = async () => await handler.HandleAsync(new ListAttachmentsQuery(10));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.NotFound);
        }

        [Fact]
        public async Task List_Should_Order_By_Id_Desc_And_Show_AuthorName_Or_Placeholder()
        {
            var tickets = new InMemoryTicketReadPort();
            tickets.Seed(new TicketSnapshot(10, TicketStatus.Novo, null, null));

            var users = new InMemoryUserReadPort();
            users.Seed(new UserSnapshot(1, "Author", "aut@x.com", "Requester"));

            var repo = new InMemoryAttachmentRepository();
            var storage = new FakeFileStoragePort();
            var clock = new FakeClock();

            var upload = new UploadAttachmentHandler(tickets, users, repo, storage, clock);

            using var content1 = new MemoryStream(new byte[100]);
            using var content2 = new MemoryStream(new byte[100]);

            var a1 = await upload.HandleAsync(
                new UploadAttachmentCommand(10, 1, new UploadFile("a1.txt", "text/plain", 100, content1)));

            var a2 = await upload.HandleAsync(
                new UploadAttachmentCommand(10, 1, new UploadFile("a2.txt", "text/plain", 100, content2)));

            var handler = new ListAttachmentsHandler(tickets, repo, users);

            var items = await handler.HandleAsync(new ListAttachmentsQuery(10));

            items.Should().HaveCount(2);
            items.First().Id.Should().BeGreaterThan(items.Last().Id);
            items.First().UploadedBy.Name.Should().Be("Author");
        }

        [Fact]
        public async Task GetById_Should_Return_404_When_Not_Found()
        {
            var repo = new InMemoryAttachmentRepository();
            var users = new InMemoryUserReadPort();

            var handler = new GetAttachmentByIdHandler(repo, users);

            var act = async () => await handler.HandleAsync(new GetAttachmentByIdQuery(10, 999));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.NotFound);
        }

        [Fact]
        public async Task GetById_Should_Return_Metadata_With_AuthorName_Or_Placeholder()
        {
            var tickets = new InMemoryTicketReadPort();
            tickets.Seed(new TicketSnapshot(10, TicketStatus.Novo, null, null));

            var users = new InMemoryUserReadPort();
            users.Seed(new UserSnapshot(1, "Author", "aut@x.com", "Requester"));

            var repo = new InMemoryAttachmentRepository();
            var storage = new FakeFileStoragePort();
            var clock = new FakeClock();

            var upload = new UploadAttachmentHandler(tickets, users, repo, storage, clock);

            using var content = new MemoryStream(new byte[100]);

            var created = await upload.HandleAsync(
                new UploadAttachmentCommand(10, 1, new UploadFile("a.txt", "text/plain", 100, content)));

            var handler = new GetAttachmentByIdHandler(repo, users);

            var dto = await handler.HandleAsync(new GetAttachmentByIdQuery(10, created.Id));

            dto.FileName.Should().Be("a.txt");
            dto.UploadedBy.Name.Should().Be("Author");
        }
    }
}