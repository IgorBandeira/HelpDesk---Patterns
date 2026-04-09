using System.Net;
using System.Net.Mail;
using HelpDesk.Application.Shared.Ports;
using HelpDesk.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpDesk.Infrastructure.Operations.Notifications.Email
{
    public sealed class SystemNetMailEmailSender : IEmailSender
    {
        private readonly SmtpOptions _opt;
        private readonly ILogger<SystemNetMailEmailSender> _logger;

        public SystemNetMailEmailSender(
            IOptions<SmtpOptions> opt,
            ILogger<SystemNetMailEmailSender> logger)
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
                    "[SystemNetMailEmailSender] DisableDelivery ativo — e-mail suprimido. Assunto: {Subject}",
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
                    "[SystemNetMailEmailSender] Sem destinatários para o assunto {Subject}",
                    message.Subject);
                return;
            }

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_opt.FromEmail, _opt.FromName),
                Subject = message.Subject,
                Body = message.HtmlBody,
                IsBodyHtml = true
            };

            foreach (var recipient in recipients)
                mailMessage.To.Add(recipient);

            using var client = new SmtpClient(_opt.Host, _opt.Port)
            {
                EnableSsl = _opt.UseStartTls,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrWhiteSpace(_opt.User))
            {
                client.Credentials = new NetworkCredential(_opt.User, _opt.Password);
            }

            ct.ThrowIfCancellationRequested();
            await client.SendMailAsync(mailMessage);

            _logger.LogInformation(
                "[SystemNetMailEmailSender] E-mail enviado para {Count} destinatários: {Recipients}",
                recipients.Count,
                string.Join(", ", recipients));
        }
    }
}