using HelpDesk.Application.Shared.Ports;
using HelpDesk.Infrastructure.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HelpDesk.Infrastructure.Operations.Notifications.Email
{
    public sealed class MailKitEmailSender : IEmailSender
    {
        private readonly SmtpOptions _opt;
        private readonly ILogger<MailKitEmailSender> _logger;

        public MailKitEmailSender(
            IOptions<SmtpOptions> opt,
            ILogger<MailKitEmailSender> logger)
        {
            _opt = opt.Value;
            _logger = logger;
        }

        public async Task SendAsync(
            EmailMessage message,
            CancellationToken ct = default)
        {
            if (_opt.DisableDelivery)
            {
                _logger.LogInformation(
                    "[MailKitEmailSender] DisableDelivery ativo — e-mail suprimido. Assunto: {Subject}",
                    message.Subject);
                return;
            }

            var recipients = message.To?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                ?? new List<string>();

            if (recipients.Count == 0)
            {
                _logger.LogWarning("Sem destinatários para o assunto {Subject}", message.Subject);
                return;
            }

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_opt.FromName, _opt.FromEmail));

            foreach (var recipient in recipients)
                mimeMessage.To.Add(MailboxAddress.Parse(recipient));

            mimeMessage.Subject = message.Subject;
            mimeMessage.Body = new BodyBuilder
            {
                HtmlBody = message.HtmlBody
            }.ToMessageBody();

            using var client = new SmtpClient();

            try
            {
                var socketOption = _opt.UseStartTls
                    ? SecureSocketOptions.StartTlsWhenAvailable
                    : SecureSocketOptions.Auto;

                await client.ConnectAsync(_opt.Host, _opt.Port, socketOption, ct);

                if (!string.IsNullOrWhiteSpace(_opt.User))
                    await client.AuthenticateAsync(_opt.User, _opt.Password, ct);

                await client.SendAsync(mimeMessage, ct);

                _logger.LogInformation(
                    "E-mail enviado para {Count} destinatários: {Recipients}",
                    recipients.Count,
                    string.Join(", ", recipients));
            }
            finally
            {
                try
                {
                    await client.DisconnectAsync(true, ct);
                }
                catch
                {
                }
            }
        }
    }
}