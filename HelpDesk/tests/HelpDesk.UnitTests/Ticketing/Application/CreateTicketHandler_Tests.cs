using FluentAssertions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.UseCases.CreateTicket;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.UnitTests.Shared.Fakes;
using HelpDesk.UnitTests.Ticketing.Fakes;

namespace HelpDesk.UnitTests.Ticketing.Application
{
    public class CreateTicketHandler_Tests
    {
        [Fact]
        public async Task Handle_Should_Require_Requester_Or_Manager()
        {
            var users = new InMemoryUserReadPort();
            users.Seed(new(10, "A", "a@x.com", "Agent"));

            var cats = new InMemoryCategoryReadPort();
            cats.Seed(1, "Cat");

            var repo = new InMemoryTicketRepository();
            var clock = new FakeClock();
            var dispatcher = new FakeDomainEventDispatcher();

            var handler = new CreateTicketHandler(repo, users, cats, clock, dispatcher);

            var act = async () => await handler.HandleAsync(new CreateTicketCommand(
                UserId: 10,
                Dto: new CreateTicketDto { Title = "T", Description = "D", Priority = TicketPriority.Media, CategoryId = 1 }
            ));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == 403);
        }

        [Fact]
        public async Task Handle_Should_Require_Category_Exists()
        {
            var users = new InMemoryUserReadPort();
            users.Seed(new(10, "R", "r@x.com", "Requester"));

            var cats = new InMemoryCategoryReadPort();
            var repo = new InMemoryTicketRepository();
            var clock = new FakeClock();
            var dispatcher = new FakeDomainEventDispatcher();

            var handler = new CreateTicketHandler(repo, users, cats, clock, dispatcher);

            var act = async () => await handler.HandleAsync(new CreateTicketCommand(
                UserId: 10,
                Dto: new CreateTicketDto { Title = "T", Description = "D", Priority = TicketPriority.Media, CategoryId = 123 }
            ));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == 400);
        }
    }
}