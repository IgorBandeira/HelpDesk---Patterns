namespace HelpDesk.Application.Attachments.Ports
{
    public sealed record UploadFile(
        string FileName,
        string? ContentType,
        long SizeBytes,
        Stream Content
    );

    public interface IFileStoragePort
    {
        Task<(string StorageKey, string? PublicUrl)> SaveAsync(UploadFile file, string key);
        Task DeleteAsync(string key);
    }
}
