using Amazon.S3;
using Amazon.S3.Model;
using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace HelpDesk.Infrastructure.Attachments.Storage
{
    public sealed class S3FileStoragePort : IFileStoragePort
    {
        private readonly IAmazonS3 _s3;
        private readonly S3Options _opts;
        private readonly ILogger<S3FileStoragePort> _logger;

        public S3FileStoragePort(
            IAmazonS3 s3,
            IOptions<S3Options> opts,
            ILogger<S3FileStoragePort> logger)
        {
            _s3 = s3;
            _opts = opts.Value;
            _logger = logger;
        }

        public async Task<(string StorageKey, string? PublicUrl)> SaveAsync(UploadFile file, string key)
        {
            var finalKey = ApplyPrefix(key);

            var put = new PutObjectRequest
            {
                BucketName = _opts.Bucket,
                Key = finalKey,
                InputStream = file.Content,
                ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                    ? "application/octet-stream"
                    : file.ContentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            put.Metadata["original-filename"] = Path.GetFileName(file.FileName);

            var resp = await _s3.PutObjectAsync(put);

            if ((int)resp.HttpStatusCode >= 300)
                throw new InvalidOperationException("Falha ao enviar ao S3.");

            var url = BuildPublicUrl(finalKey);

            _logger.LogInformation("Arquivo enviado ao S3. Key={Key}", finalKey);

            return (finalKey, url);
        }

        public async Task DeleteAsync(string key)
        {
            var finalKey = key.Trim().TrimStart('/');

            var resp = await _s3.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _opts.Bucket,
                Key = finalKey
            });

            if ((int)resp.HttpStatusCode is < 200 or >= 300)
                throw new InvalidOperationException("Falha ao excluir arquivo no S3.");
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
            var baseUrl = _opts.PublicBaseUrl.TrimEnd('/');
            return $"{baseUrl}/{encodedKey}";
        }

        private static string UrlEncodePerSegment(string key)
        {
            var segments = key.Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(WebUtility.UrlEncode);

            return string.Join('/', segments);
        }
    }
}