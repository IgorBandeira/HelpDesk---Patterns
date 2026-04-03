using FluentAssertions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.UseCases.UpdateTicket;
using HelpDesk.Domain.Ticketing.Aggregates;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.Domain.Ticketing.ValueObjects;
using HelpDesk.UnitTests.Shared.Fakes;
using HelpDesk.UnitTests.Ticketing.Fakes;

namespace HelpDesk.UnitTests.Ticketing.Application
{
    public class UpdateTicketHandler_Tests
    {
        [Fact]
        public async Task Handle_Should_Require_Owner_Or_Manager()
        {
            var users = new InMemoryUserReadPort();
            users.Seed(new(10, "R", "r@x.com", "Requester"));
            users.Seed(new(11, "Other", "o@x.com", "Requester"));

            var cats = new InMemoryCategoryReadPort();
            cats.Seed(1, "Cat");

            var repo = new InMemoryTicketRepository();
            var clock = new FakeClock();
            var dispatcher = new FakeDomainEventDispatcher();

            var t = Ticket.CreateNew(
                TicketTitle.Create("T"),
                TicketDescription.Create("D"),
                TicketPriority.Media,
                requesterId: 10,
                categoryId: 1,
                clock.Now);

            repo.Seed(t);

            var handler = new UpdateTicketHandler(repo, users, cats, clock, dispatcher);

            var act = async () => await handler.HandleAsync(new UpdateTicketCommand(
                Id: t.Id,
                UserId: 11,
                Dto: new UpdateTicketDto { Title = "New" }
            ));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == 403);
        }

        [Fact]
        public async Task Handle_Should_Return_400_When_No_Changes()
        {
            var users = new InMemoryUserReadPort();
            users.Seed(new(10, "R", "r@x.com", "Requester"));

            var cats = new InMemoryCategoryReadPort();
            cats.Seed(1, "Cat");

            var repo = new InMemoryTicketRepository();
            var clock = new FakeClock();
            var dispatcher = new FakeDomainEventDispatcher();

            var t = Ticket.CreateNew(
                TicketTitle.Create("T"),
                TicketDescription.Create("D"),
                TicketPriority.Media,
                10,
                1,
                clock.Now);

            repo.Seed(t);

            var handler = new UpdateTicketHandler(repo, users, cats, clock, dispatcher);

            var act = async () => await handler.HandleAsync(new UpdateTicketCommand(
                Id: t.Id,
                UserId: 10,
                Dto: new UpdateTicketDto()
            ));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == 400);
        }
    }
}