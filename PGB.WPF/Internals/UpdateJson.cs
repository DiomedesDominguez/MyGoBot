namespace PGB.WPF.Internals
{
    using Newtonsoft.Json;

    internal class UpdateJson
    {
        [JsonProperty(PropertyName = "download_url")]
        public string DownloadUrl { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }
    }
}