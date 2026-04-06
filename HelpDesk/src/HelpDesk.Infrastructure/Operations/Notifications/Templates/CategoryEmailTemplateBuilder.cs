using System;
using System.Text;

namespace HelpDesk.Infrastructure.Operations.Notifications.Templates
{
    public sealed class CategoryEmailTemplateBuilder
    {
        public string BuildCreatedSubject(int categoryId)
            => $"[HelpDesk] Categoria #{categoryId} criada";

        public string BuildDeletedSubject(int categoryId)
            => $"[HelpDesk] Categoria #{categoryId} excluída";

        public string BuildCreatedEmail(
            int categoryId,
            string name,
            string? parentName,
            string actorUserName,
            DateTime occurredAt)
        {
            var title = BuildCreatedSubject(categoryId);

            var body = $"""
                <p>Uma categoria foi <strong>criada</strong> no sistema.</p>
                <p><strong>Executado por:</strong> {HtmlEncode(actorUserName)}</p>
                """;

            var middle = BuildCategoryPaperCard(
                categoryId,
                name,
                parentName,
                occurredAt,
                accentColor: "#1d4ed8",
                accentLabel: "Criação");

            return HtmlLayout(title, body, middle);
        }

        public string BuildDeletedEmail(
            int categoryId,
            string name,
            string? parentName,
            string actorUserName,
            DateTime occurredAt)
        {
            var title = BuildDeletedSubject(categoryId);

            var body = $"""
                <p>Uma categoria foi <strong>excluída</strong> do sistema.</p>
                <p><strong>Executado por:</strong> {HtmlEncode(actorUserName)}</p>
                """;

            var middle = BuildCategoryPaperCard(
                categoryId,
                name,
                parentName,
                occurredAt,
                accentColor: "#b91c1c",
                accentLabel: "Exclusão");

            return HtmlLayout(title, body, middle);
        }

        private string BuildCategoryPaperCard(
            int categoryId,
            string name,
            string? parentName,
            DateTime occurredAt,
            string accentColor,
            string accentLabel)
        {
            var occurredAtText = occurredAt.ToString("dd/MM/yyyy HH:mm");
            var parentText = string.IsNullOrWhiteSpace(parentName) ? "Sem" : parentName;

            return $"""
                <div style="background:#f8f8f8;border:1px solid #ddd;border-left:6px solid {accentColor};
                            border-radius:8px;padding:12px;margin:16px 0">
                  <div style="margin:4px 0"><strong style="color:{accentColor}">Ação:</strong> {HtmlEncode(accentLabel)}</div>
                  <div style="margin:4px 0"><strong style="color:{accentColor}">Id:</strong> {categoryId}</div>
                  <div style="margin:4px 0"><strong style="color:{accentColor}">Nome:</strong> {HtmlEncode(name)}</div>
                  <div style="margin:4px 0"><strong style="color:{accentColor}">Categoria pai:</strong> {HtmlEncode(parentText)}</div>
                  <div style="margin:4px 0"><strong style="color:{accentColor}">Data:</strong> {occurredAtText}</div>
                </div>
                """;
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