using HelpDesk.Application.Ticketing.Ports;

namespace HelpDesk.UnitTests.Ticketing.Fakes
{
    public sealed class FakeNotificationPort : INotificationPort
    {
        public List<(int TicketId, string Description, string? ExtraEmail)> ActionsSent { get; } = new();
        public List<int> SlaAlertsSent { get; } = new();

        public Task NotifyTicketActionAsync(
            int ticketId,
            string description,
            string? extraEmail = null,
            CancellationToken ct = default)
        {
            ActionsSent.Add((ticketId, description, extraEmail));
            return Task.CompletedTask;
        }

        public Task NotifySlaAlertAsync(
            int ticketId,
            CancellationToken ct = default)
        {
            SlaAlertsSent.Add(ticketId);
            return Task.CompletedTask;
        }
    }
}
