namespace PGB.WPF.Internals
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    internal static class HttpProvider
    {
        public static Task<HttpResponseMessage> Get(string url)
        {
            return new HttpClient().GetAsync(url);
        }

        public static Task<HttpResponseMessage> Post(string url, Dictionary<string, string> keyValuePairs)
        {
            return new HttpClient().PostAsync(url, new FormUrlEncodedContent(keyValuePairs));
        }
    }
}