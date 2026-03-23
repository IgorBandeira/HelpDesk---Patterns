using HelpDesk.Application.Attachments.Ports;

namespace HelpDesk.IntegrationTests.Fakes
{
    public sealed class FakeFileStoragePort : IFileStoragePort
    {
        public List<string> SavedKeys { get; } = new();
        public List<string> DeletedKeys { get; } = new();

        public Task<(string StorageKey, string? PublicUrl)> SaveAsync(UploadFile file, string key)
        {
            SavedKeys.Add(key);
            return Task.FromResult<(string, string?)>((key, $"https://fake-storage.local/{key}"));
        }

        public Task DeleteAsync(string key)
        {
            DeletedKeys.Add(key);
            return Task.CompletedTask;
        }

        public void Reset()
        {
            SavedKeys.Clear();
            DeletedKeys.Clear();
        }
    }
}