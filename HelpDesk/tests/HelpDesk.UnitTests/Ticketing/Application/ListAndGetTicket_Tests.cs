using FluentAssertions;
using HelpDesk.Application.Shared.DTOs;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.UseCases.GetTicketById;
using HelpDesk.Application.Ticketing.UseCases.ListTickets;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.UnitTests.Shared.Fakes;
using HelpDesk.UnitTests.Ticketing.Fakes;

namespace HelpDesk.UnitTests.Ticketing.Application
{
    public class ListAndGetTicket_Tests
    {
        [Fact]
        public async Task Handle_Should_Exclude_Canceled_By_Default()
        {
            var queryPort = new InMemoryTicketQueryPort();
            var now = new DateTime(2026, 1, 1, 10, 0, 0);

            queryPort.SeedList(
                new TicketListItemDto(
                    1,
                    "A",
                    TicketStatus.Novo,
                    TicketPriority.Media,
                    now,
                    now.AddHours(48),
                    new UserMiniDto(1, "Requester 1"),
                    null,
                    new CategoryMiniDto(1, "Cat 1")),
                new TicketListItemDto(
                    2,
                    "B",
                    TicketStatus.Cancelado,
                    TicketPriority.Media,
                    now,
                    now.AddHours(48),
                    new UserMiniDto(1, "Requester 1"),
                    null,
                    new CategoryMiniDto(1, "Cat 1"))
            );

            var handler = new ListTicketsHandler(queryPort);

            var result = await handler.HandleAsync(new ListTicketsQuery(
                Status: null, Priority: null, Title: null,
                CreatedFrom: null, CreatedTo: null,
                RequesterId: null, AssigneeId: null, CategoryId: null,
                SlaDueFrom: null, SlaDueTo: null, OverdueOnly: false,
                Page: 1, PageSize: 20));

            result.Should().ContainSingle(x => x.Title == "A");
            result.Should().NotContain(x => x.Title == "B");
        }

        [Fact]
        public async Task Handle_Should_Filter_By_Status_When_Informed()
        {
            var queryPort = new InMemoryTicketQueryPort();
            var now = new DateTime(2026, 1, 1, 10, 0, 0);

            queryPort.SeedList(
                new TicketListItemDto(
                    1,
                    "T",
                    TicketStatus.EmAnalise,
                    TicketPriority.Media,
                    now,
                    now.AddHours(48),
                    new UserMiniDto(1, "Requester 1"),
                    new UserMiniDto(99, "Agent 99"),
                    new CategoryMiniDto(1, "Cat 1"))
            );

            var handler = new ListTicketsHandler(queryPort);

            var result = await handler.HandleAsync(new ListTicketsQuery(
                Status: TicketStatus.EmAnalise, Priority: null, Title: null,
                CreatedFrom: null, CreatedTo: null,
                RequesterId: null, AssigneeId: null, CategoryId: null,
                SlaDueFrom: null, SlaDueTo: null, OverdueOnly: false,
                Page: 1, PageSize: 20));

            result.Should().ContainSingle(x => x.Status == TicketStatus.EmAnalise);
        }

        [Fact]
        public async Task Handle_Should_Return_401_When_User_Not_Found()
        {
            var users = new InMemoryUserReadPort();
            var queryPort = new InMemoryTicketQueryPort();

            var handler = new GetTicketByIdHandler(users, queryPort);

            var act = async () => await handler.HandleAsync(new GetTicketByIdQuery(Id: 1, UserId: 10));

            (await act.Should().ThrowAsync<AppException>())
                .Where(e => e.StatusCode == 401);
        }
    }
}