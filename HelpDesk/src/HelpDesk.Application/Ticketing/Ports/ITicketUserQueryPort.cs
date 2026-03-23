namespace HelpDesk.Application.Ticketing.Ports
{
    public interface ITicketUserQueryPort
    {
        Task<bool> HasActiveTicketsAsRequesterAsync(int userId);
        Task<bool> HasActiveTicketsAsAssigneeAsync(int userId);

        Task<IReadOnlyList<UserTicketListItem>> ListRequestedTicketsAsync(int userId);
        Task<IReadOnlyList<UserTicketListItem>> ListAssignedTicketsAsync(int userId);
    }

    public sealed record UserTicketListItem(int Id, string Title, string Status, string PriorityLevel);
}
