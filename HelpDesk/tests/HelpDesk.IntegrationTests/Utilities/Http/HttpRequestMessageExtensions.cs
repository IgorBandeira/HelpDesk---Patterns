namespace HelpDesk.IntegrationTests.Utilities.Http
{
    public static class HttpRequestMessageExtensions
    {
        public static HttpRequestMessage WithUserId(this HttpRequestMessage req, int userId)
        {
            req.Headers.Remove("userId");
            req.Headers.Add("userId", userId.ToString());
            return req;
        }
    }
}
