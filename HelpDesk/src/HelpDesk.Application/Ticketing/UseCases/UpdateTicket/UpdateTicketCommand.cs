namespace HelpDesk.Application.Ticketing.UseCases.UpdateTicket
{
    public sealed record UpdateTicketCommand(int Id, int UserId, DTOs.UpdateTicketDto Dto);
}
