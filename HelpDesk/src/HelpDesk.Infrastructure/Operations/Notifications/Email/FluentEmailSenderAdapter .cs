using System.Net;
using System.Net.Mail;
using FluentEmail.Smtp;
using HelpDesk.Application.Shared.Ports;
using HelpDesk.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpDesk.Infrastructure.Operations.Notifications.Email
{
    public sealed class FluentEmailSenderAdapter : IEmailSender
    {
        private readonly SmtpOptions _opt;
        private readonly ILogger<FluentEmailSenderAdapter> _logger;

        public FluentEmailSenderAdapter(
            IOptions<SmtpOptions> opt,
            ILogger<FluentEmailSenderAdapter> logger)
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
                    "[FluentEmailSenderAdapter] DisableDelivery ativo — e-mail suprimido. Assunto: {Subject}",
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
                    "[FluentEmailSenderAdapter] Sem destinatários para o assunto {Subject}",
                    message.Subject);
                return;
            }

            using var smtpClient = new SmtpClient(_opt.Host, _opt.Port)
            {
                EnableSsl = _opt.UseStartTls,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrWhiteSpace(_opt.User))
            {
                smtpClient.Credentials = new NetworkCredential(_opt.User, _opt.Password);
            }

            var sender = new SmtpSender(() => smtpClient);

            var email = FluentEmail.Core.Email
                .From(new MailAddress(_opt.FromEmail, _opt.FromName).ToString())
                .Subject(message.Subject)
                .Body(message.HtmlBody, isHtml: true);

            foreach (var recipient in recipients)
                email.To(recipient);

            ct.ThrowIfCancellationRequested();

            var response = await sender.SendAsync(email);

            if (!response.Successful)
            {
                var errors = string.Join(" | ", response.ErrorMessages);
                _logger.LogError(
                    "[FluentEmailSenderAdapter] Falha ao enviar e-mail. Assunto: {Subject}. Erros: {Errors}",
                    message.Subject,
                    errors);

                throw new InvalidOperationException(
                    $"Falha ao enviar e-mail com FluentEmail: {errors}");
            }

            _logger.LogInformation(
                "[FluentEmailSenderAdapter] E-mail enviado para {Count} destinatários: {Recipients}",
                recipients.Count,
                string.Join(", ", recipients));
        }
    }
}