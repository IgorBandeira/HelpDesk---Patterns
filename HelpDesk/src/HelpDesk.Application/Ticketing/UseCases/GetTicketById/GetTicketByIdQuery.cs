namespace HelpDesk.Application.Ticketing.UseCases.GetTicketById
{
    public sealed record GetTicketByIdQuery(int Id, int UserId);
}