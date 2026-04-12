using Aspose.Email;
using Aspose.Email.Clients;
using Aspose.Email.Clients.Smtp;
using HelpDesk.Application.Operations.Ports.Email;
using HelpDesk.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpDesk.Infrastructure.Operations.Notifications.Email
{
    public sealed class AsposeEmailSender : IEmailSender
    {
        private readonly SmtpOptions _opt;
        private readonly ILogger<AsposeEmailSender> _logger;

        public AsposeEmailSender(
            IOptions<SmtpOptions> opt,
            ILogger<AsposeEmailSender> logger)
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
                    "[AsposeEmailSender] DisableDelivery ativo — e-mail suprimido. Assunto: {Subject}",
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
                _logger.LogWarning(
                    "[AsposeEmailSender] Sem destinatários para o assunto {Subject}",
                    message.Subject);
                return;
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_opt.FromEmail, _opt.FromName),
                Subject = message.Subject,
                HtmlBody = message.HtmlBody
            };

            foreach (var recipient in recipients)
                mailMessage.To.Add(new MailAddress(recipient));

            using var client = new SmtpClient(_opt.Host, _opt.Port, _opt.User, _opt.Password)
            {
                SecurityOptions = _opt.UseStartTls
                    ? SecurityOptions.Auto
                    : SecurityOptions.None
            };

            ct.ThrowIfCancellationRequested();
            await Task.Run(() => client.Send(mailMessage), ct);

            _logger.LogInformation(
                "[AsposeEmailSender] E-mail enviado para {Count} destinatários: {Recipients}",
                recipients.Count,
                string.Join(", ", recipients));
        }
    }
}