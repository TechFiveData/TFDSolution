using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TFDIntegrations.EWayBill.Helpers
{
    public class ApiClientHelper
    {
        private readonly HttpClient _client;
        public ApiClientHelper()
        {
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromSeconds(60);
        }

        public async Task<string> PostAsync(string url, string json, string authToken = null)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                if (!string.IsNullOrEmpty(authToken))
                {
                    request.Headers.Add("authtoken", authToken);
                }
                var response = await _client.SendAsync(request);
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}