namespace HelpDesk.Application.Shared.Ports
{
    public sealed record EmailMessage(
        IReadOnlyCollection<string> To,
        string Subject,
        string HtmlBody);

    public interface IEmailSender
    {
        Task SendAsync(EmailMessage message, CancellationToken ct = default);
    }
}