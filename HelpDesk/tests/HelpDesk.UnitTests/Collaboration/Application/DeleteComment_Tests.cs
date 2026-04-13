using FluentAssertions;
using HelpDesk.Application.Collaboration.DTOs;
using HelpDesk.Domain.Collaboration.ValueObjects;
using HelpDesk.Domain.Collaboration.Aggregates;
using HelpDesk.Application.Collaboration.UseCases.AddComment;
using HelpDesk.Application.Collaboration.UseCases.DeleteComment;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Domain.Collaboration.Enums;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.UnitTests.Collaboration.Fakes;
using HelpDesk.UnitTests.Shared.Fakes;

namespace HelpDesk.UnitTests.Collaboration.Application
{
    public class ReplaceAndDeleteComment_Tests
    {
        [Fact]
        public async Task Delete_Should_Be_Allowed_Only_For_Author()
        {
            // Arrange
            var users = new InMemoryUserReadPort();
            users.Seed(new(1, "Author", "aut@.com", "Requester"));
            users.Seed(new(2, "Other", "out@.com", "Requester"));

            var tickets = new InMemoryTicketReadPort();
            tickets.Seed(new(Id: 10, Status: TicketStatus.Novo, RequesterId: 1, AssigneeId: null));

            var repo = new InMemoryCommentRepository();
            var clock = new FakeClock();
            var dispatcher = new FakeDomainEventDispatcher();

            var add = new AddCommentHandler(users, tickets, repo, clock, dispatcher);
            var created = await add.HandleAsync(new AddCommentCommand(
                10, 1, new AddCommentDto("x", CommentVisibility.Public)));

            var handler = new DeleteCommentHandler(tickets, repo, users, clock, dispatcher);

            // Act
            var actOther = async () => await handler.HandleAsync(new DeleteCommentCommand(10, created.Id, 2));

            // Assert
            (await actOther.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.Forbidden);

            // Act
            var actAuthor = async () => await handler.HandleAsync(new DeleteCommentCommand(10, created.Id, 1));

            // Assert
            await actAuthor.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Delete_Should_Block_When_Ticket_Is_Inactive()
        {
            // Arrange
            var users = new InMemoryUserReadPort();
            users.Seed(new(1, "Author", "aut@.com", "Requester"));

            var tickets = new InMemoryTicketReadPort();
            tickets.Seed(new(Id: 10, Status: TicketStatus.Cancelado, RequesterId: 1, AssigneeId: null));

            var repo = new InMemoryCommentRepository();
            var clock = new FakeClock();
            var dispatcher = new FakeDomainEventDispatcher();

            var comment = TicketComment.CreateNew(
                ticketId: 10,
                authorId: 1,
                visibility: CommentVisibility.Public,
                message: CommentMessage.Create("x"),
                now: clock.Now);

            await repo.AddAsync(comment);

            var handler = new DeleteCommentHandler(tickets, repo, users, clock, dispatcher);

            // Act
            var act = async () => await handler.HandleAsync(new DeleteCommentCommand(10, comment.Id, 1));

            // Assert
            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.BadRequest);
        }
    }
}