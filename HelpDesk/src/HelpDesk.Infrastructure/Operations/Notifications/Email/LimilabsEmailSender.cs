using System.Security.Authentication;
using HelpDesk.Application.Shared.Ports;
using HelpDesk.Infrastructure.Options;
using Limilabs.Client.SMTP;
using Limilabs.Mail;
using Limilabs.Mail.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpDesk.Infrastructure.Operations.Notifications.Email
{
    public sealed class LimilabsEmailSender : IEmailSender
    {
        private readonly SmtpOptions _opt;
        private readonly ILogger<LimilabsEmailSender> _logger;

        public LimilabsEmailSender(
            IOptions<SmtpOptions> opt,
            ILogger<LimilabsEmailSender> logger)
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
                    "[LimilabsEmailSender] DisableDelivery ativo — e-mail suprimido. Assunto: {Subject}",
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
                    "[LimilabsEmailSender] Sem destinatários para o assunto {Subject}",
                    message.Subject);
                return;
            }

            var builder = new MailBuilder
            {
                Subject = message.Subject,
                Html = message.HtmlBody
            };

            builder.From.Add(new MailBox(_opt.FromEmail, _opt.FromName));

            foreach (var recipient in recipients)
                builder.To.Add(new MailBox(recipient));

            var mail = builder.Create();

            using var smtp = new Smtp();

            try
            {
                smtp.SSLConfiguration.EnabledSslProtocols = SslProtocols.Tls12;

                await Task.Run(() => smtp.Connect(_opt.Host, _opt.Port), ct);

                if (_opt.UseStartTls)
                {
                    await Task.Run(() => smtp.StartTLS(), ct);
                }

                if (!string.IsNullOrWhiteSpace(_opt.User))
                {
                    await Task.Run(() => smtp.UseBestLogin(_opt.User, _opt.Password), ct);
                }

                await Task.Run(() => smtp.SendMessage(mail), ct);

                _logger.LogInformation(
                    "[LimilabsEmailSender] E-mail enviado para {Count} destinatários: {Recipients}",
                    recipients.Count,
                    string.Join(", ", recipients));
            }
            finally
            {
                try
                {
                    smtp.Close();
                }
                catch
                {
                }
            }
        }
    }
}