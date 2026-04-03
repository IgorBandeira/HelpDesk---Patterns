using FluentAssertions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.UseCases.ChangeRequester;
using HelpDesk.Domain.Ticketing.Aggregates;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.Domain.Ticketing.ValueObjects;
using HelpDesk.UnitTests.Shared.Fakes;
using HelpDesk.UnitTests.Ticketing.Fakes;

namespace HelpDesk.UnitTests.Ticketing.Application
{
    public class ChangeRequesterHandler_Tests
    {
        [Fact]
        public async Task Handle_Should_Require_New_Requester_Role_Requester()
        {
            var users = new InMemoryUserReadPort();
            users.Seed(new(10, "Owner", "o@x.com", "Requester"));
            users.Seed(new(20, "New", "n@x.com", "Agent"));

            var repo = new InMemoryTicketRepository();
            var clock = new FakeClock();
            var dispatcher = new FakeDomainEventDispatcher();

            var t = Ticket.CreateNew(TicketTitle.Create("T"), TicketDescription.Create("D"), TicketPriority.Media, 10, 1, clock.Now);
            repo.Seed(t);

            var handler = new ChangeRequesterHandler(repo, users, clock, dispatcher);

            var act = async () => await handler.HandleAsync(new ChangeRequesterCommand(
                Id: t.Id,
                UserId: 10,
                Dto: new ChangeRequesterDto { RequesterId = 20 }
            ));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == 400);
        }
    }
}