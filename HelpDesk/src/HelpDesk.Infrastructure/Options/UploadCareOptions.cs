namespace HelpDesk.Infrastructure.Options
{
    public sealed class UploadCareOptions
    {
        public string PublicKey { get; set; } = "";
        public string SecretKey { get; set; } = "";
        public string Store { get; set; } = "1";
        public string PublicBaseUrl { get; set; } = "";
        public string Prefix { get; set; } = "";
    }
}