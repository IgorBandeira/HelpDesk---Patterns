using HelpDesk.Application.Ticketing.DTOs;

namespace HelpDesk.Application.Ticketing.UseCases.ReopenTicket
{
    public sealed record ReopenTicketCommand(int Id, int UserId,ReopenTicketDto Dto);

}
