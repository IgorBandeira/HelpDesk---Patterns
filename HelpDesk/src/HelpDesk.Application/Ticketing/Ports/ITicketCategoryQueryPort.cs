namespace HelpDesk.Application.Ticketing.Ports
{
    public interface ITicketCategoryQueryPort
    {
        Task<bool> HasActiveTicketsForCategoryAsync(int categoryId);
    }
}
