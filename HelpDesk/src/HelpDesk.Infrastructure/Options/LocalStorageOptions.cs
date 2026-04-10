namespace HelpDesk.Infrastructure.Options
{
    public sealed class LocalStorageOptions
    {
        public string RootPath { get; set; } = "";
        public string PublicBaseUrl { get; set; } = "";
        public string Prefix { get; set; } = "";
    }
}