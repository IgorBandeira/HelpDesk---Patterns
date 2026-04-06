using System;
using System.Collections.Generic;
using System.Text;

namespace HelpDesk.Infrastructure.Operations.Notifications.Templates
{
    public sealed class UserEmailTemplateBuilder
    {
        public string BuildCreatedSubject(int userId)
            => $"[HelpDesk] Usuário #{userId} criado";

        public string BuildUpdatedSubject(int userId)
            => $"[HelpDesk] Usuário #{userId} atualizado";

        public string BuildDeletedSubject(int userId)
            => $"[HelpDesk] Usuário #{userId} excluído";

        public string BuildCreatedEmail(
            int userId,
            string name,
            string email,
            string role,
            string actorUserName,
            DateTime occurredAt)
        {
            var title = BuildCreatedSubject(userId);

            var body = $"""
                <p>Um usuário foi <strong>criado</strong> no sistema.</p>
                <p><strong>Executado por:</strong> {HtmlEncode(actorUserName)}</p>
                """;

            var middle = BuildUserPaperCard(
                userId,
                name,
                email,
                role,
                occurredAt,
                accentColor: "#1d4ed8",
                accentLabel: "Criação");

            return HtmlLayout(title, body, middle);
        }

        public string BuildUpdatedEmail(
            int userId,
            string actorUserName,
            IReadOnlyList<string> changes,
            DateTime occurredAt)
        {
            var title = BuildUpdatedSubject(userId);

            var body = $"""
                <p>Um usuário foi <strong>atualizado</strong> no sistema.</p>
                <p><strong>Executado por:</strong> {HtmlEncode(actorUserName)}</p>
                <p><strong>Alterações realizadas:</strong></p>
                {BuildChangesList(changes)}
                """;

            var middle = BuildSimpleMetaCard(
                userId,
                occurredAt,
                accentColor: "#b45309",
                accentLabel: "Atualização");

            return HtmlLayout(title, body, middle);
        }

        public string BuildDeletedEmail(
            int userId,
            string name,
            string email,
            string role,
            string actorUserName,
            DateTime occurredAt)
        {
            var title = BuildDeletedSubject(userId);

            var body = $"""
                <p>Um usuário foi <strong>excluído</strong> do sistema.</p>
                <p><strong>Executado por:</strong> {HtmlEncode(actorUserName)}</p>
                """;

            var middle = BuildUserPaperCard(
                userId,
                name,
                email,
                role,
                occurredAt,
                accentColor: "#b91c1c",
                accentLabel: "Exclusão");

            return HtmlLayout(title, body, middle);
        }

        private string BuildUserPaperCard(
            int userId,
            string name,
            string email,
            string role,
            DateTime occurredAt,
            string accentColor,
            string accentLabel)
        {
            var occurredAtText = occurredAt.ToString("dd/MM/yyyy HH:mm");

            return $"""
                <div style="background:#f8f8f8;border:1px solid #ddd;border-left:6px solid {accentColor};
                            border-radius:8px;padding:12px;margin:16px 0">
                  <div style="margin:4px 0"><strong style="color:{accentColor}">Ação:</strong> {HtmlEncode(accentLabel)}</div>
                  <div style="margin:4px 0"><strong style="color:{accentColor}">Id:</strong> {userId}</div>
                  <div style="margin:4px 0"><strong style="color:{accentColor}">Nome:</strong> {HtmlEncode(name)}</div>
                  <div style="margin:4px 0"><strong style="color:{accentColor}">E-mail:</strong> {HtmlEncode(email)}</div>
                  <div style="margin:4px 0"><strong style="color:{accentColor}">Papel:</strong> {HtmlEncode(role)}</div>
                  <div style="margin:4px 0"><strong style="color:{accentColor}">Data:</strong> {occurredAtText}</div>
                </div>
                """;
        }

        private string BuildSimpleMetaCard(
            int userId,
            DateTime occurredAt,
            string accentColor,
            string accentLabel)
        {
            var occurredAtText = occurredAt.ToString("dd/MM/yyyy HH:mm");

            return $"""
                <div style="background:#f8f8f8;border:1px solid #ddd;border-left:6px solid {accentColor};
                            border-radius:8px;padding:12px;margin:16px 0">
                  <div style="margin:4px 0"><strong style="color:{accentColor}">Ação:</strong> {HtmlEncode(accentLabel)}</div>
                  <div style="margin:4px 0"><strong style="color:{accentColor}">Id:</strong> {userId}</div>
                  <div style="margin:4px 0"><strong style="color:{accentColor}">Data:</strong> {occurredAtText}</div>
                </div>
                """;
        }

        private string BuildChangesList(IReadOnlyList<string> changes)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<ul>");

            foreach (var change in changes)
            {
                sb.AppendLine($"<li>{HtmlEncode(change)}</li>");
            }

            sb.AppendLine("</ul>");
            return sb.ToString();
        }

        public string HtmlLayout(string title, string body, string? middleBlockHtml = null)
        {
            var sb = new StringBuilder();

            sb.Append($"""
                <div style="font-family:Arial,Helvetica,sans-serif;font-size:14px">
                  <h2 style="margin:0 0 12px 0">{HtmlEncode(title)}</h2>
                  <div>{body}</div>
                  <hr style="margin:16px 0;"/>
                """);

            if (!string.IsNullOrWhiteSpace(middleBlockHtml))
                sb.Append(middleBlockHtml);

            sb.Append("""
                  <hr style="margin:16px 0;"/>
                  <div style="color:#666;font-size:12px;text-align:right">
                    Mensagem automática do HelpDesk - NoReply
                  </div>
                </div>
                """);

            return sb.ToString();
        }

        private static string HtmlEncode(string value)
            => System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
    }
}