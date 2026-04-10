using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpDesk.Infrastructure.Attachments.Storage
{
    public sealed class LocalFileStoragePort : IFileStoragePort
    {
        private readonly LocalStorageOptions _opts;
        private readonly ILogger<LocalFileStoragePort> _logger;

        public LocalFileStoragePort(
            IOptions<LocalStorageOptions> opts,
            ILogger<LocalFileStoragePort> logger)
        {
            _opts = opts.Value;
            _logger = logger;
        }

        public async Task<(string StorageKey, string? PublicUrl)> SaveAsync(UploadFile file, string key)
        {
            var finalKey = ApplyPrefix(key);
            var fullPath = BuildAbsolutePath(finalKey);

            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            if (file.Content.CanSeek)
                file.Content.Position = 0;

            await using var output = new FileStream(
                fullPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true);

            await file.Content.CopyToAsync(output);

            var url = BuildPublicUrl(finalKey);

            _logger.LogInformation("Arquivo salvo localmente. Path={Path}", fullPath);

            return (finalKey, url);
        }

        public Task DeleteAsync(string key)
        {
            var storedKey = key.Trim().TrimStart('/');
            var fullPath = BuildAbsolutePath(storedKey);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Arquivo removido localmente. Path={Path}", fullPath);
            }

            return Task.CompletedTask;
        }

        private string ApplyPrefix(string key)
        {
            var cleanKey = key.Trim().TrimStart('/');

            if (string.IsNullOrWhiteSpace(_opts.Prefix))
                return cleanKey;

            return $"{_opts.Prefix.Trim().Trim('/')}/{cleanKey}";
        }

        private string BuildAbsolutePath(string key)
        {
            var normalized = key.Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(_opts.RootPath, normalized);
        }

        private string BuildPublicUrl(string key)
        {
            var fullPath = BuildAbsolutePath(key);

            if (string.IsNullOrWhiteSpace(_opts.PublicBaseUrl))
                return new Uri(fullPath).AbsoluteUri;

            return $"{_opts.PublicBaseUrl.TrimEnd('/')}/{key.Replace("\\", "/").TrimStart('/')}";
        }
    }
}