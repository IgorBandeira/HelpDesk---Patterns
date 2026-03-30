namespace HelpDesk.Application.Operations.Ports
{
    public interface INotificationPort
    {
        Task NotifyTicketActionAsync(
            int ticketId,
            string description,
            string? extraEmail = null,
            CancellationToken ct = default);

        Task NotifySlaAlertAsync(
            int ticketId,
            CancellationToken ct = default);
    }
}
