using System.Text;
using HelpDesk.Infrastructure.Ticketing.Models;

namespace HelpDesk.Infrastructure.Ticketing.Notifications.Templates
{
    public sealed class TicketEmailTemplateBuilder
    {
        public string BuildActionEmail(
            int ticketId,
            string description,
            TicketEntity? ticket,
            string? categoryName)
        {
            var descTrim = description?.Trim() ?? string.Empty;

            string subjectCore;
            if (descTrim.StartsWith("Título do chamado alterado", StringComparison.OrdinalIgnoreCase))
                subjectCore = "Título do chamado alterado.";
            else if (descTrim.StartsWith("Descrição do chamado alterada", StringComparison.OrdinalIgnoreCase))
                subjectCore = "Descrição do chamado alterada.";
            else
                subjectCore = descTrim;

            var title = $"[HelpDesk] Ticket #{ticketId} — {subjectCore}";
            var body = $"<p>{HtmlEncode(descTrim)}</p>";

            string? middle = null;
            if (ticket is not null)
                middle = BuildTicketPaperCard(ticket, categoryName, isAlert: false);

            return HtmlLayout(title, body, middle);
        }

        public string BuildSlaAlertEmail(TicketEntity ticket, string? categoryName)
        {
            var title = $"⚠️ [HelpDesk] Alerta de SLA — Ticket #{ticket.Id}";
            var body = $@"
            <p>O ticket <strong style=""color:#d00000"">#{ticket.Id}</strong>
            (“<strong style=""color:#d00000"">{HtmlEncode(ticket.Title)}</strong>”)
            está <strong style=""color:#d00000"">próximo do vencimento de SLA</strong>.</p>
            <p>Por favor, priorize a resolução antes do prazo final.</p>";

            var middle = BuildTicketPaperCard(ticket, categoryName, isAlert: true);
            return HtmlLayout(title, body, middle);
        }

        public string BuildActionSubject(int ticketId, string description)
        {
            var descTrim = description?.Trim() ?? string.Empty;

            string subjectCore;
            if (descTrim.StartsWith("Título do chamado alterado", StringComparison.OrdinalIgnoreCase))
                subjectCore = "Título do chamado alterado.";
            else if (descTrim.StartsWith("Descrição do chamado alterada", StringComparison.OrdinalIgnoreCase))
                subjectCore = "Descrição do chamado alterada.";
            else
                subjectCore = descTrim;

            return $"[HelpDesk] Ticket #{ticketId} — {subjectCore}";
        }

        public string BuildSlaAlertSubject(int ticketId)
        {
            return $"⚠️ [HelpDesk] Alerta de SLA — Ticket #{ticketId}";
        }

        public string BuildTicketPaperCard(
            TicketEntity ticket,
            string? categoryName,
            bool isAlert)
        {
            var safeCategoryName = string.IsNullOrWhiteSpace(categoryName)
                ? "(categoria removida)"
                : categoryName;

            var created = ticket.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            var due = ticket.SlaDueAt.HasValue ? ticket.SlaDueAt.Value.ToString("dd/MM/yyyy HH:mm") : "-";
            var desc = string.IsNullOrWhiteSpace(ticket.Description) ? "-" : HtmlEncode(ticket.Description);

            var bgColor = isAlert ? "#ffe6e6" : "#f5f5f5";
            var borderColor = isAlert ? "#ff4d4d" : "#ddd";
            var strongColor = isAlert ? "#d00000" : "#000";

            return $@"
            <div style=""background:{bgColor};border:1px solid {borderColor};
                        border-radius:8px;padding:12px;margin:16px 0"">
              <div style=""margin:4px 0""><strong style=""color:{strongColor}"">Título:</strong> {HtmlEncode(ticket.Title)}</div>
              <div style=""margin:4px 0""><strong style=""color:{strongColor}"">Status:</strong> {HtmlEncode(ticket.Status)}</div>
              <div style=""margin:4px 0""><strong style=""color:{strongColor}"">Prioridade:</strong> {HtmlEncode(ticket.PriorityLevel)}</div>
              <div style=""margin:4px 0""><strong style=""color:{strongColor}"">Categoria:</strong> {HtmlEncode(safeCategoryName)}</div>
              <div style=""margin:4px 0""><strong style=""color:{strongColor}"">Criado em:</strong> {created}</div>
              <div style=""margin:4px 0""><strong style=""color:{strongColor}"">Vence em:</strong> {due}</div>
              <div style=""margin:4px 0""><strong style=""color:{strongColor}"">Descrição:</strong></div>
              <div style=""white-space:pre-wrap;line-height:1.35"">{desc}</div>
            </div>";
        }

        public string HtmlLayout(string title, string body, string? middleBlockHtml = null)
        {
            var sb = new StringBuilder();

            sb.Append($@"
            <div style=""font-family:Arial,Helvetica,sans-serif;font-size:14px"">
              <h2 style=""margin:0 0 12px 0"">{title}</h2>
              <div>{body}</div>
              <hr style=""margin:16px 0;""/>");

            if (!string.IsNullOrWhiteSpace(middleBlockHtml))
                sb.Append(middleBlockHtml);

            sb.Append(@"
              <hr style=""margin:16px 0;""/>
              <div style=""color:#666;font-size:12px;text-align:right"">
                Mensagem automática do HelpDesk - NoReply
              </div>
            </div>");

            return sb.ToString();
        }

        private static string HtmlEncode(string value)
            => System.Net.WebUtility.HtmlEncode(value);
    }
}