using FluentAssertions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.UseCases.AssignTicket;
using HelpDesk.Domain.Ticketing.Aggregates;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.Domain.Ticketing.ValueObjects;
using HelpDesk.UnitTests.Shared.Fakes;
using HelpDesk.UnitTests.Ticketing.Fakes;

namespace HelpDesk.UnitTests.Ticketing.Application
{
    public class AssignTicketHandler_Tests
    {
        [Fact]
        public async Task Handle_Should_Require_Owner_Or_Manager()
        {
            var users = new InMemoryUserReadPort();
            users.Seed(new(10, "R", "r@x.com", "Requester"));
            users.Seed(new(11, "Other", "o@x.com", "Requester"));
            users.Seed(new(99, "Agent", "a@x.com", "Agent"));

            var cats = new InMemoryCategoryReadPort();
            cats.Seed(1, "Cat");

            var repo = new InMemoryTicketRepository();
            var clock = new FakeClock();
            var dispatcher = new FakeDomainEventDispatcher();

            var t = Ticket.CreateNew(TicketTitle.Create("T"), TicketDescription.Create("D"), TicketPriority.Media, 10, 1, clock.Now);
            repo.Seed(t);

            var handler = new AssignTicketHandler(repo, users, clock, dispatcher);

            var act = async () => await handler.HandleAsync(new AssignTicketCommand(
                Id: t.Id,
                UserId: 11,
                Dto: new AssignRequestDto { AgentId = 99 }
            ));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == 403);
        }

        [Fact]
        public async Task Handle_Should_Require_Target_User_Is_Agent()
        {
            var users = new InMemoryUserReadPort();
            users.Seed(new(10, "R", "r@x.com", "Requester"));
            users.Seed(new(99, "NotAgent", "na@x.com", "Requester"));

            var repo = new InMemoryTicketRepository();
            var clock = new FakeClock();
            var dispatcher = new FakeDomainEventDispatcher();

            var t = Ticket.CreateNew(TicketTitle.Create("T"), TicketDescription.Create("D"), TicketPriority.Media, 10, 1, clock.Now);
            repo.Seed(t);

            var handler = new AssignTicketHandler(repo, users, clock, dispatcher);

            var act = async () => await handler.HandleAsync(new AssignTicketCommand(
                Id: t.Id,
                UserId: 10,
                Dto: new AssignRequestDto { AgentId = 99 }
            ));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == 400);
        }
    }
}