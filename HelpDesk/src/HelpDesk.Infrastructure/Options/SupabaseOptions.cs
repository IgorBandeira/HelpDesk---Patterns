namespace HelpDesk.Infrastructure.Options
{
    public sealed class SupabaseOptions
    {
        public string ProjectUrl { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public string Bucket { get; set; } = "";
        public string Prefix { get; set; } = "";
        public string PublicBaseUrl { get; set; } = "";
    }
}