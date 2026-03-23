namespace HelpDesk.Application.Ticketing.UseCases.CreateTicket
{
    public sealed record CreateTicketCommand(int UserId, DTOs.CreateTicketDto Dto);

}
