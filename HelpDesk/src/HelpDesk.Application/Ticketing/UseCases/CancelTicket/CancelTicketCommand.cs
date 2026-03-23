namespace HelpDesk.Application.Ticketing.UseCases.CancelTicket
{
    public sealed record CancelTicketCommand(int Id, int UserId, DTOs.CancelTicketDto Dto);

}
