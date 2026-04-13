using FluentAssertions;
using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Application.Attachments.UseCases.DeleteAttachment;
using HelpDesk.Application.Attachments.UseCases.UploadAttachment;
using HelpDesk.Domain.Attachments.Aggregates;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Domain.Attachments.ValueObjects;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.UnitTests.Attachments.Fakes;
using HelpDesk.UnitTests.Shared.Fakes;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Application.IdentityAccess.Ports;

namespace HelpDesk.UnitTests.Attachments.Application
{
    public class DeleteAttachmentHandler_Tests
    {
        [Fact]
        public async Task Delete_Should_Be_Allowed_Only_For_Author()
        {
            var tickets = new InMemoryTicketReadPort();
            tickets.Seed(new TicketSnapshot(10, TicketStatus.Novo, null, null));

            var users = new InMemoryUserReadPort();
            users.Seed(new UserSnapshot(1, "Author", "aut@x.com", "Requester"));
            users.Seed(new UserSnapshot(2, "Other", "out@x.com", "Requester"));

            var repo = new InMemoryAttachmentRepository();
            var storage = new FakeFileStoragePort();
            var clock = new FakeClock();
            var dispatcher = new FakeDomainEventDispatcher();

            var upload = new UploadAttachmentHandler(tickets, users, repo, storage, clock, dispatcher);

            using var content = new MemoryStream(new byte[100]);
            var file = new UploadFile("a.txt", "text/plain", 100, content);

            var created = await upload.HandleAsync(
                new UploadAttachmentCommand(10, 1, file));

            var handler = new DeleteAttachmentHandler(tickets, repo, users, clock, dispatcher);

            var actOther = async () => await handler.HandleAsync(new DeleteAttachmentCommand(10, created.Id, 2));

            (await actOther.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.Forbidden);

            var actAuthor = async () => await handler.HandleAsync(new DeleteAttachmentCommand(10, created.Id, 1));

            await actAuthor.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Delete_Should_Block_When_Ticket_Inactive()
        {
            var tickets = new InMemoryTicketReadPort();
            tickets.Seed(new TicketSnapshot(10, TicketStatus.Cancelado, null, null));

            var users = new InMemoryUserReadPort();
            users.Seed(new UserSnapshot(1, "Author", "aut@x.com", "Requester"));

            var repo = new InMemoryAttachmentRepository();
            var clock = new FakeClock();
            var dispatcher = new FakeDomainEventDispatcher();

            var now = new DateTime(2026, 1, 1, 10, 0, 0);
            var att = Attachment.CreateNew(
                ticketId: 10,
                file: AttachmentFile.Create("a.txt", "text/plain", 100),
                storageKey: StorageKey.Create("tickets/10/a.txt"),
                publicUrl: "http://x",
                uploadedById: 1,
                now: now);

            await repo.AddAsync(att);

            var handler = new DeleteAttachmentHandler(tickets, repo, users, clock, dispatcher);

            var act = async () => await handler.HandleAsync(new DeleteAttachmentCommand(10, att.Id, 1));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.BadRequest);
        }
    }
}