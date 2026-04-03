using FluentAssertions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.UseCases.ChangeStatus;
using HelpDesk.Domain.Ticketing.Aggregates;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.Domain.Ticketing.ValueObjects;
using HelpDesk.UnitTests.Shared.Fakes;
using HelpDesk.UnitTests.Ticketing.Fakes;

namespace HelpDesk.UnitTests.Ticketing.Application
{
    public class ChangeStatusHandler_Tests
    {
        [Fact]
        public async Task Handle_Should_Require_Assignee_For_Agent_Transitions()
        {
            var users = new InMemoryUserReadPort();
            users.Seed(new(10, "Owner", "o@x.com", "Requester"));
            users.Seed(new(99, "Agent", "a@x.com", "Agent"));

            var repo = new InMemoryTicketRepository();
            var clock = new FakeClock();
            var dispatcher = new FakeDomainEventDispatcher();

            var t = Ticket.CreateNew(TicketTitle.Create("T"), TicketDescription.Create("D"), TicketPriority.Media, 10, 1, clock.Now);
            repo.Seed(t);

            t.AssignToAgent(agentId: 99, clock.Now.AddMinutes(1));

            var handler = new ChangeStatusHandler(repo, users, clock, dispatcher);

            var act = async () => await handler.HandleAsync(new ChangeStatusCommand(
                Id: t.Id,
                UserId: 10,
                Dto: new ChangeStatusDto { NewStatus = TicketStatus.EmAndamento }
            ));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == 403);
        }
    }
}