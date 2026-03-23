using FluentAssertions;
using HelpDesk.Application.Collaboration.DTOs;
using HelpDesk.Application.Collaboration.UseCases.AddComment;
using HelpDesk.Application.Collaboration.UseCases.GetCommentById;
using HelpDesk.Application.Collaboration.UseCases.ListComments;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Domain.Collaboration.Enums;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.UnitTests.Collaboration.Fakes;
using HelpDesk.UnitTests.Shared.Fakes;

namespace HelpDesk.UnitTests.Collaboration.Application
{
    public class ListAndGetComment_Tests
    {
        [Fact]
        public async Task List_Should_Hide_Internal_Comments_From_NonParticipants()
        {
            var users = new InMemoryUserReadPort();
            users.Seed(new(1, "Req", "req@.com", "Requester"));
            users.Seed(new(2, "Out", "out@.com", "Requester"));
            var tickets = new InMemoryTicketReadPort();
            tickets.Seed(new(Id: 10, Status: TicketStatus.Novo, RequesterId: 1, AssigneeId: null));
            var repo = new InMemoryCommentRepository();
            var commentReads = new InMemoryCommentReadPort(repo);
            var clock = new FakeClock();

            var add = new AddCommentHandler(users, tickets, repo, clock);
            await add.HandleAsync(new AddCommentCommand(10, 1, new AddCommentDto("interno", CommentVisibility.Internal)));
            await add.HandleAsync(new AddCommentCommand(10, 1, new AddCommentDto("publico", CommentVisibility.Public)));

            var handler = new ListCommentsHandler(users, tickets, commentReads);

            var outsiderItems = await handler.HandleAsync(new ListCommentsQuery(10, 2));
            outsiderItems.Should().ContainSingle();
            outsiderItems.First().Visibility.Should().Be(CommentVisibility.Public);
        }

        [Fact]
        public async Task GetById_Should_Hide_Internal_From_NonParticipants_And_Allow_Public()
        {
            var users = new InMemoryUserReadPort();
            users.Seed(new(1, "Req", "req@.com", "Requester"));
            users.Seed(new(2, "Out", "out@.com", "Requester"));
            var tickets = new InMemoryTicketReadPort();
            tickets.Seed(new(Id: 10, Status: TicketStatus.Novo, RequesterId: 1, AssigneeId: null));
            var repo = new InMemoryCommentRepository();
            var commentReads = new InMemoryCommentReadPort(repo);
            var clock = new FakeClock();

            var add = new AddCommentHandler(users, tickets, repo, clock);
            var internalCreated = await add.HandleAsync(
                new AddCommentCommand(10, 1, new AddCommentDto("interno", CommentVisibility.Internal)));
            var publicCreated = await add.HandleAsync(
                new AddCommentCommand(10, 1, new AddCommentDto("publico", CommentVisibility.Public)));

            var get = new GetCommentByIdHandler(users, tickets, commentReads);

            var actInternal = async () => await get.HandleAsync(
                new GetCommentByIdQuery(TicketId: 10, CommentId: internalCreated.Id, UserId: 2));
            (await actInternal.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == HttpStatusCodes.NotFound);

            var dto = await get.HandleAsync(
                new GetCommentByIdQuery(TicketId: 10, CommentId: publicCreated.Id, UserId: 2));
            dto.Id.Should().Be(publicCreated.Id);
            dto.Visibility.Should().Be(CommentVisibility.Public);
            dto.Message.Should().Be("publico");
        }
    }
}