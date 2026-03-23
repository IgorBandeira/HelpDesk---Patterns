using FluentAssertions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.UseCases.ReopenTicket;
using HelpDesk.Domain.Ticketing.Aggregates;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.Domain.Ticketing.ValueObjects;
using HelpDesk.UnitTests.Shared.Fakes;
using HelpDesk.UnitTests.Ticketing.Fakes;

namespace HelpDesk.UnitTests.Ticketing.Application
{
    public class ReopenTicketHandler_Tests
    {
        [Fact]
        public async Task Handle_Should_Require_Reason()
        {
            var users = new InMemoryUserReadPort();
            users.Seed(new(10, "Owner", "o@x.com", "Requester"));
            users.Seed(new(99, "Agent", "a@x.com", "Agent"));

            var repo = new InMemoryTicketRepository();
            var clock = new FakeClock();

            var t = Ticket.CreateNew(TicketTitle.Create("T"), TicketDescription.Create("D"), TicketPriority.Media, 10, 1, clock.Now);
            repo.Seed(t);

            t.AssignToAgent(99, clock.Now.AddMinutes(1));
            t.ChangeStatus(TicketStatus.EmAndamento, 99, clock.Now.AddMinutes(2));
            t.ChangeStatus(TicketStatus.Resolvido, 99, clock.Now.AddMinutes(3));
            t.ChangeStatus(TicketStatus.Fechado, 10, clock.Now.AddMinutes(4));

            var handler = new ReopenTicketHandler(repo, users, clock, notify: new FakeNotificationPort());

            var act = async () => await handler.HandleAsync(new ReopenTicketCommand(
                Id: t.Id,
                UserId: 10,
                Dto: new ReopenTicketDto { Reason = "   " }
            ));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == 400);
        }
    }
}
