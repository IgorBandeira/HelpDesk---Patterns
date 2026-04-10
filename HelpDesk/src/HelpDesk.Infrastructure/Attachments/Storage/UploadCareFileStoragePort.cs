using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HelpDesk.Infrastructure.Attachments.Storage
{
    public sealed class UploadCareFileStoragePort : IFileStoragePort
    {
        private const string UploadApiBase = "https://upload.uploadcare.com";
        private const string RestApiBase = "https://api.uploadcare.com";

        private readonly HttpClient _httpClient;
        private readonly UploadCareOptions _opts;
        private readonly ILogger<UploadCareFileStoragePort> _logger;

        public UploadCareFileStoragePort(
            HttpClient httpClient,
            IOptions<UploadCareOptions> opts,
            ILogger<UploadCareFileStoragePort> logger)
        {
            _httpClient = httpClient;
            _opts = opts.Value;
            _logger = logger;
        }

        public async Task<(string StorageKey, string? PublicUrl)> SaveAsync(UploadFile file, string key)
        {
            var finalKey = ApplyPrefix(key);

            if (file.Content.CanSeek)
                file.Content.Position = 0;

            using var form = new MultipartFormDataContent();

            form.Add(new StringContent(_opts.PublicKey), "UPLOADCARE_PUB_KEY");
            form.Add(new StringContent(_opts.Store), "UPLOADCARE_STORE");

            // Nome final exibido no CDN / dashboard
            var fileName = ExtractFileName(finalKey, file.FileName);

            var streamContent = new StreamContent(file.Content);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(file.ContentType)
                    ? "application/octet-stream"
                    : file.ContentType);

            form.Add(streamContent, "file", fileName);

            var response = await _httpClient.PostAsync($"{UploadApiBase}/base/", form);

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"Falha ao enviar arquivo para Uploadcare. Status={(int)response.StatusCode}. Body={body}");

            var uploadResponse = JsonSerializer.Deserialize<UploadcareUploadResponse>(
                body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (uploadResponse is null || string.IsNullOrWhiteSpace(uploadResponse.File))
                throw new InvalidOperationException("Uploadcare não retornou o UUID do arquivo.");

            var storageKey = $"{finalKey}|{uploadResponse.File}";
            var publicUrl = BuildPublicUrl(uploadResponse.File, fileName);

            _logger.LogInformation(
                "Arquivo enviado ao Uploadcare. StorageKey={StorageKey}, Uuid={Uuid}",
                storageKey,
                uploadResponse.File);

            return (storageKey, publicUrl);
        }

        public async Task DeleteAsync(string key)
        {
            var parsed = ParseStorageKey(key);

            if (string.IsNullOrWhiteSpace(parsed.Uuid))
                throw new InvalidOperationException(
                    "StorageKey inválida para Uploadcare. Esperado: '<caminho>|<uuid>'.");

            using var request = new HttpRequestMessage(
                HttpMethod.Delete,
                $"{RestApiBase}/files/{parsed.Uuid}/storage/");

            request.Headers.TryAddWithoutValidation(
                "Accept",
                "application/vnd.uploadcare-v0.7+json");

            request.Headers.TryAddWithoutValidation(
                "Authorization",
                $"Uploadcare.Simple {_opts.PublicKey}:{_opts.SecretKey}");

            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"Falha ao excluir arquivo no Uploadcare. Status={(int)response.StatusCode}. Body={body}");

            _logger.LogInformation("Arquivo removido do Uploadcare. Uuid={Uuid}", parsed.Uuid);
        }

        private string ApplyPrefix(string key)
        {
            var cleanKey = key.Trim().TrimStart('/');

            if (string.IsNullOrWhiteSpace(_opts.Prefix))
                return cleanKey;

            return $"{_opts.Prefix.Trim().Trim('/')}/{cleanKey}";
        }

        private string BuildPublicUrl(string uuid, string fileName)
        {
            var encodedFileName = Uri.EscapeDataString(fileName);
            var baseUrl = string.IsNullOrWhiteSpace(_opts.PublicBaseUrl)
                ? "https://ucarecdn.com"
                : _opts.PublicBaseUrl.TrimEnd('/');

            return $"{baseUrl}/{uuid}/{encodedFileName}";
        }

        private static string ExtractFileName(string finalKey, string fallbackFileName)
        {
            var normalized = finalKey.Replace("\\", "/").Trim('/');
            var name = normalized.Split('/').LastOrDefault();

            return string.IsNullOrWhiteSpace(name)
                ? Path.GetFileName(fallbackFileName)
                : name;
        }

        private static ParsedStorageKey ParseStorageKey(string storageKey)
        {
            var parts = storageKey.Split('|', 2, StringSplitOptions.TrimEntries);

            return parts.Length == 2
                ? new ParsedStorageKey(parts[0], parts[1])
                : new ParsedStorageKey(storageKey, "");
        }

        private sealed record UploadcareUploadResponse(string File);
        private sealed record ParsedStorageKey(string LogicalPath, string Uuid);
    }
}