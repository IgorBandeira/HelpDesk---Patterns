using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace HelpDesk.Infrastructure.Attachments.Storage
{
    public sealed class SupabaseFileStoragePort : IFileStoragePort
    {
        private readonly HttpClient _httpClient;
        private readonly SupabaseOptions _opts;
        private readonly ILogger<SupabaseFileStoragePort> _logger;

        public SupabaseFileStoragePort(
            HttpClient httpClient,
            IOptions<SupabaseOptions> opts,
            ILogger<SupabaseFileStoragePort> logger)
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

            using var content = new StreamContent(file.Content);
            content.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(file.ContentType)
                    ? "application/octet-stream"
                    : file.ContentType);

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_opts.ProjectUrl.TrimEnd('/')}/storage/v1/object/{_opts.Bucket}/{finalKey}");

            request.Headers.Add("apikey", _opts.ApiKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _opts.ApiKey);
            request.Headers.Add("x-upsert", "true");
            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Falha ao enviar arquivo para o Supabase. Status={(int)response.StatusCode}. Body={body}");
            }

            var url = BuildPublicUrl(finalKey);

            _logger.LogInformation("Arquivo enviado ao Supabase Storage. Key={Key}", finalKey);

            return (finalKey, url);
        }

        public async Task DeleteAsync(string key)
        {
            var storedKey = key.Trim().TrimStart('/');

            var json = $$"""
            {
              "prefixes": ["{{storedKey}}"]
            }
            """;

            using var request = new HttpRequestMessage(
                HttpMethod.Delete,
                $"{_opts.ProjectUrl.TrimEnd('/')}/storage/v1/object/{_opts.Bucket}");

            request.Headers.Add("apikey", _opts.ApiKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _opts.ApiKey);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Falha ao excluir arquivo no Supabase. Status={(int)response.StatusCode}. Body={body}");
            }

            _logger.LogInformation("Arquivo removido do Supabase Storage. Key={Key}", storedKey);
        }

        private string ApplyPrefix(string key)
        {
            var cleanKey = key.Trim().TrimStart('/');

            if (string.IsNullOrWhiteSpace(_opts.Prefix))
                return cleanKey;

            return $"{_opts.Prefix.Trim().Trim('/')}/{cleanKey}";
        }

        private string BuildPublicUrl(string key)
        {
            var encodedKey = UrlEncodePerSegment(key);

            if (!string.IsNullOrWhiteSpace(_opts.PublicBaseUrl))
                return $"{_opts.PublicBaseUrl.TrimEnd('/')}/{encodedKey}";

            return $"{_opts.ProjectUrl.TrimEnd('/')}/storage/v1/object/public/{_opts.Bucket}/{encodedKey}";
        }

        private static string UrlEncodePerSegment(string key)
        {
            var segments = key
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.EscapeDataString);

            return string.Join('/', segments);
        }
    }
}