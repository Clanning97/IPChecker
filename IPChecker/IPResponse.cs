using System.Text.Json.Serialization;

namespace IPChecker
{
    public class IPResponse
    {
        [JsonPropertyName("ip")]
        public string IP { get; set; }
    }
}