using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpDesk.Infrastructure.Attachments.Storage
{
    public sealed class CloudinaryFileStoragePort : IFileStoragePort
    {
        private readonly Cloudinary _cloudinary;
        private readonly CloudinaryOptions _opts;
        private readonly ILogger<CloudinaryFileStoragePort> _logger;

        public CloudinaryFileStoragePort(
            IOptions<CloudinaryOptions> opts,
            ILogger<CloudinaryFileStoragePort> logger)
        {
            _opts = opts.Value;
            _logger = logger;

            var account = new Account(_opts.CloudName, _opts.ApiKey, _opts.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<(string StorageKey, string? PublicUrl)> SaveAsync(UploadFile file, string key)
        {
            var finalKey = ApplyPrefix(key);
            var requestedPublicId = BuildPublicId(finalKey);

            if (file.Content.CanSeek)
                file.Content.Position = 0;

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, file.Content),
                PublicId = requestedPublicId,
                Folder = string.IsNullOrWhiteSpace(_opts.Folder) ? null : _opts.Folder.Trim('/'),
                Overwrite = true
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error is not null)
                throw new InvalidOperationException(
                    $"Falha ao enviar arquivo para Cloudinary: {result.Error.Message}");

            if (string.IsNullOrWhiteSpace(result.PublicId))
                throw new InvalidOperationException("Cloudinary não retornou o PublicId do arquivo.");

            _logger.LogInformation(
                "Arquivo enviado ao Cloudinary. RequestedPublicId={RequestedPublicId}, ReturnedPublicId={ReturnedPublicId}, Url={Url}",
                requestedPublicId,
                result.PublicId,
                result.SecureUrl);

            return (result.PublicId, result.SecureUrl?.ToString());
        }

        public async Task DeleteAsync(string key)
        {
            var publicId = key.Trim().Trim('/');

            var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Raw,
                Invalidate = true
            });

            _logger.LogInformation(
                "Cloudinary delete. PublicId={PublicId}, Result={Result}, Error={Error}",
                publicId,
                result.Result,
                result.Error?.Message);

            if (result.Error is not null)
                throw new InvalidOperationException(
                    $"Falha ao excluir arquivo no Cloudinary: {result.Error.Message}");

            if (result.Result is not "ok" and not "not found")
                throw new InvalidOperationException(
                    $"Falha ao excluir arquivo no Cloudinary. PublicId={publicId}, Result={result.Result}");
        }

        private string ApplyPrefix(string key)
        {
            var cleanKey = key.Trim().TrimStart('/');

            if (string.IsNullOrWhiteSpace(_opts.Prefix))
                return cleanKey;

            return $"{_opts.Prefix.Trim().Trim('/')}/{cleanKey}";
        }

        private string BuildPublicId(string key)
        {
            return key.Replace("\\", "/").Trim('/');
        }
    }
}