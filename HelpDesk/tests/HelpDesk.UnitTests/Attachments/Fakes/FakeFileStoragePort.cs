using HelpDesk.Application.Attachments.Ports;

namespace HelpDesk.UnitTests.Attachments.Fakes
{
    public sealed class FakeFileStoragePort : IFileStoragePort
    {
        public List<string> DeletedKeys { get; } = new();
        public List<(string Key, UploadFile File)> Saved { get; } = new();

        public Task<(string StorageKey, string? PublicUrl)> SaveAsync(UploadFile file, string key)
        {
            Saved.Add((key, file));
            // return exactly the key like controller
            return Task.FromResult((StorageKey: key, PublicUrl: "http://public/url"));
        }

        public Task DeleteAsync(string key)
        {
            DeletedKeys.Add(key);
            return Task.CompletedTask;
        }
    }
}
