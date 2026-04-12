namespace HelpDesk.Application.Operations.Ports.Email
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