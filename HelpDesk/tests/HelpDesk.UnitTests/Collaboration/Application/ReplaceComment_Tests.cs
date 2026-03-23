using FluentAssertions;
using HelpDesk.Application.Collaboration.DTOs;
using HelpDesk.Application.Collaboration.UseCases.AddComment;
using HelpDesk.Domain.Collaboration.ValueObjects;
using HelpDesk.Application.Collaboration.UseCases.ReplaceCommentMessage;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Domain.Collaboration.Enums;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.Domain.Collaboration.Aggregates;
using HelpDesk.UnitTests.Collaboration.Fakes;
using HelpDesk.UnitTests.Shared.Fakes;

namespace HelpDesk.UnitTests.Collaboration.Application
{
    public class ReplaceComment_Tests
    {
        [Fact]
        public async Task Replace_Should_Be_Allowed_Only_For_Author()
        {
            var users = new InMemoryUserReadPort();
            users.Seed(new(1, "Author", "aut@.com", "Requester"));
            users.Seed(new(2, "Other", "out@.com", "Requester"));
            var tickets = new InMemoryTicketReadPort();
            tickets.Seed(new(Id: 10, Status: TicketStatus.Novo, RequesterId: 1, AssigneeId: null));
            var repo = new InMemoryCommentRepository();
            var commentReads = new InMemoryCommentReadPort(repo);
            var clock = new FakeClock();

            var add = new AddCommentHandler(users, tickets, repo, clock);
            var created = await add.HandleAsync(new AddCommentCommand(
                10, 1, new AddCommentDto("x", CommentVisibility.Public)));

            var handler = new ReplaceCommentMessageHandler(tickets, repo, commentReads, clock);

            var actOther = async () => await handler.HandleAsync(
                new ReplaceCommentMessageCommand(10, created.Id, 2, new UpdateCommentMessageDto("hack")));

            (await actOther.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.Forbidden);

            var updated = await handler.HandleAsync(
                new ReplaceCommentMessageCommand(10, created.Id, 1, new UpdateCommentMessageDto("novo")));

            updated.Message.Should().StartWith("editado: ");
        }

        [Fact]
        public async Task Replace_Should_Block_When_Ticket_Is_Inactive()
        {
            var tickets = new InMemoryTicketReadPort();
            tickets.Seed(new(Id: 10, Status: TicketStatus.Fechado, RequesterId: 1, AssigneeId: null));
            var repo = new InMemoryCommentRepository();
            var commentReads = new InMemoryCommentReadPort(repo);
            var clock = new FakeClock();

            var comment = TicketComment.CreateNew(
                ticketId: 10,
                authorId: 1,
                visibility: CommentVisibility.Public,
                message: CommentMessage.Create("x"),
                now: clock.Now);
            await repo.AddAsync(comment);

            var handler = new ReplaceCommentMessageHandler(tickets, repo, commentReads, clock);

            var act = async () => await handler.HandleAsync(
                new ReplaceCommentMessageCommand(10, comment.Id, 1, new UpdateCommentMessageDto("novo")));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.BadRequest);
        }
    }
}
