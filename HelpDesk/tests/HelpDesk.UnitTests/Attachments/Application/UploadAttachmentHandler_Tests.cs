using FluentAssertions;
using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Application.Attachments.UseCases.UploadAttachment;
using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.UnitTests.Attachments.Fakes;
using HelpDesk.UnitTests.Shared.Fakes;

namespace HelpDesk.UnitTests.Attachments.Application
{
    public class UploadAttachmentHandler_Tests
    {
        [Fact]
        public async Task Upload_Should_Validate_File_And_Ticket_Active_And_User_Exists()
        {
            var tickets = new InMemoryTicketReadPort();
            tickets.Seed(new TicketSnapshot(10, TicketStatus.Novo, null, null));

            var users = new InMemoryUserReadPort();
            users.Seed(new UserSnapshot(1, "U", "u@x.com", "Requester"));

            var repo = new InMemoryAttachmentRepository();
            var storage = new FakeFileStoragePort();
            var clock = new FakeClock();

            var handler = new UploadAttachmentHandler(tickets, users, repo, storage, clock);

            using var blockedContent = new MemoryStream(new byte[100]);
            var blockedFile = new UploadFile("virus.exe", "application/octet-stream", 100, blockedContent);

            var actBlocked = async () => await handler.HandleAsync(
                new UploadAttachmentCommand(10, 1, blockedFile));

            (await actBlocked.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.BadRequest);

            using var okContent = new MemoryStream(new byte[100]);
            var okFile = new UploadFile("a.txt", "text/plain", 100, okContent);

            var created = await handler.HandleAsync(
                new UploadAttachmentCommand(10, 1, okFile));

            created.TicketId.Should().Be(10);
            created.UploadedById.Should().Be(1);
            created.StorageKey.Should().Be("10/a.txt");
        }

        [Fact]
        public async Task Upload_Should_Return_404_When_Ticket_Not_Found()
        {
            var tickets = new InMemoryTicketReadPort();

            var users = new InMemoryUserReadPort();
            users.Seed(new UserSnapshot(1, "U", "u@x.com", "Requester"));

            var repo = new InMemoryAttachmentRepository();
            var storage = new FakeFileStoragePort();
            var clock = new FakeClock();

            var handler = new UploadAttachmentHandler(tickets, users, repo, storage, clock);

            using var content = new MemoryStream(new byte[100]);
            var file = new UploadFile("a.txt", "text/plain", 100, content);

            var act = async () => await handler.HandleAsync(
                new UploadAttachmentCommand(10, 1, file));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.NotFound);
        }

        [Fact]
        public async Task Upload_Should_Return_400_When_Ticket_Inactive()
        {
            var tickets = new InMemoryTicketReadPort();
            tickets.Seed(new TicketSnapshot(10, TicketStatus.Fechado, null, null));

            var users = new InMemoryUserReadPort();
            users.Seed(new UserSnapshot(1, "U", "u@x.com", "Requester"));

            var repo = new InMemoryAttachmentRepository();
            var storage = new FakeFileStoragePort();
            var clock = new FakeClock();

            var handler = new UploadAttachmentHandler(tickets, users, repo, storage, clock);

            using var content = new MemoryStream(new byte[100]);
            var file = new UploadFile("a.txt", "text/plain", 100, content);

            var act = async () => await handler.HandleAsync(
                new UploadAttachmentCommand(10, 1, file));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.BadRequest);
        }
    }
}