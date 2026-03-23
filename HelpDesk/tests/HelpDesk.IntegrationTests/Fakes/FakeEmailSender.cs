using HelpDesk.Application.Shared.Ports;

namespace HelpDesk.IntegrationTests.Fakes
{
    public sealed class FakeEmailSender : IEmailSender
    {
        public Task SendAsync(EmailMessage message, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }
}