using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Transport.Integration;

namespace TFDIntegrations.EWayBill.Services
{
    /// <summary>
    /// Common HTTP client wrapper for NIC E-Way Bill API communication.
    /// </summary>
    public class HttpService
    {
        private readonly HttpClient _client;
        public HttpService()
        {
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromSeconds(120);
        }

        /// <summary>
        /// Sends POST request with JSON payload.
        /// </summary>
        public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest request, string authToken = null)
        {
            try
            {
                string json = JsonConvert.SerializeObject(request);
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
                if (!string.IsNullOrEmpty(authToken))
                {
                    httpRequest.Headers.Add("authtoken", authToken);
                }
                HttpResponseMessage response = await _client.SendAsync(httpRequest);
                string responseText = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(responseText);
                }
                return JsonConvert.DeserializeObject<TResponse>(responseText);
            }
            catch (Exception ex)
            {
                throw new Exception("E-Way Bill API communication failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Sends raw encrypted payload.
        /// </summary>
        public async Task<string> PostRawAsync(string url, string querytype, string encryptedData, string authToken, IntegrationConfiguration _configuration)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url + "?action=" + querytype);

            request.Content = new StringContent(encryptedData, Encoding.UTF8, "application/json");

            request.Headers.Add("aspid", _configuration.EWayBill_AspId);
            request.Headers.Add("password", _configuration.EWayBill_AspPassword);
            request.Headers.Add("Gstin", _configuration.GSTIN);
            request.Headers.Add("username", _configuration.EWayBill_EWBUsername);
            request.Headers.Add("ewbpwd", _configuration.EWayBill_EWBPassword);
            request.Headers.Add("authtoken", authToken);

            HttpResponseMessage response = await _client.SendAsync(request);

            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> GetRawAsync(string url, string querytype, string authToken, IntegrationConfiguration _configuration)
        {
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                url + "?action=" + querytype + "&authtoken=" + authToken
            );

            request.Headers.Add("aspid", _configuration.EWayBill_AspId);
            request.Headers.Add("password", _configuration.EWayBill_AspPassword);
            request.Headers.Add("Gstin", _configuration.GSTIN);
            request.Headers.Add("username", _configuration.EWayBill_EWBUsername);
            request.Headers.Add("ewbpwd", _configuration.EWayBill_EWBPassword);
            request.Headers.Add("authtoken", authToken);

            HttpResponseMessage response = await _client.SendAsync(request);

            return await response.Content.ReadAsStringAsync();
        }
    }
}