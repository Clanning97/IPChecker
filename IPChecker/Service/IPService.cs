using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace IPChecker.Service
{
    public interface IIPService
    {
        Task<string> GetIP();
    }

    public class IPService : IIPService
    {
        private readonly HttpClient _client;

        public IPService(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> GetIP()
        {
            var response = await _client.GetFromJsonAsync<IPResponse>("https://api.ipify.org?format=json");

            return response.IP;
        }
    }
}