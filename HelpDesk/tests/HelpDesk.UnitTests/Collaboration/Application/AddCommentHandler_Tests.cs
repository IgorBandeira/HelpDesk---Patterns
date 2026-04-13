using FluentAssertions;
using HelpDesk.Application.Collaboration.DTOs;
using HelpDesk.Application.Collaboration.UseCases.AddComment;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Domain.Collaboration.Enums;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.UnitTests.Collaboration.Fakes;
using HelpDesk.UnitTests.Shared.Fakes;

namespace HelpDesk.UnitTests.Collaboration.Application
{
    public class AddCommentHandler_Tests
    {
        [Fact]
        public async Task Add_Internal_Comment_Should_Require_Participation()
        {
            var users = new InMemoryUserReadPort();
            users.Seed(new(1, "Req", "req@.com", "Requester"));
            users.Seed(new(2, "Out", "out@.com", "Requester"));

            var tickets = new InMemoryTicketReadPort();
            tickets.Seed(new(Id: 10, Status: TicketStatus.Novo, RequesterId: 1, AssigneeId: null));

            var repo = new InMemoryCommentRepository();
            var clock = new FakeClock();
            var dispatcher = new FakeDomainEventDispatcher();

            var handler = new AddCommentHandler(users, tickets, repo, clock, dispatcher);

            var allowed = await handler.HandleAsync(new AddCommentCommand(
                TicketId: 10,
                UserId: 1,
                Dto: new AddCommentDto("interno", CommentVisibility.Internal)));

            allowed.Visibility.Should().Be(CommentVisibility.Internal);

            var act = async () => await handler.HandleAsync(new AddCommentCommand(
                TicketId: 10,
                UserId: 2,
                Dto: new AddCommentDto("interno", CommentVisibility.Internal)));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.Forbidden);
        }

        [Fact]
        public async Task Add_Should_Block_When_Ticket_Is_Closed_Or_Canceled()
        {
            var users = new InMemoryUserReadPort();
            users.Seed(new(1, "Req", "req@.com", "Requester"));

            var tickets = new InMemoryTicketReadPort();
            tickets.Seed(new(Id: 10, Status: TicketStatus.Fechado, RequesterId: 1, AssigneeId: null));

            var repo = new InMemoryCommentRepository();
            var clock = new FakeClock();
            var dispatcher = new FakeDomainEventDispatcher();

            var handler = new AddCommentHandler(users, tickets, repo, clock, dispatcher);

            var act = async () => await handler.HandleAsync(new AddCommentCommand(
                10, 1, new AddCommentDto("x", CommentVisibility.Public)));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.BadRequest);
        }
    }
}