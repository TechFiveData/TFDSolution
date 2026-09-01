using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TFDIntegrations.EWayBill.Helpers;
using TFDIntegrations.EWayBill.Models;
using TFDIntegrations.EWayBill.Models.Request;
using TFDIntegrations.EWayBill.Models.Response;
using TFDSolution.Transport.Integration;

namespace TFDIntegrations.EWayBill.Services
{
    /// <summary>
    /// Main E-Way Bill business service.
    /// </summary>
    public class EWayBillService
    {
        private readonly HttpService _httpService;
        private readonly EncryptionService _encryptionService;
        private readonly AuthenticationService _authenticationService;
        private readonly string _ewayBillUrl;
        private readonly IntegrationConfiguration _configuration;

        public EWayBillService(
            HttpService httpService,
            EncryptionService encryptionService,
            AuthenticationService authenticationService,
            string ewayBillUrl, IntegrationConfiguration configuration)
        {
            _httpService = httpService;
            _encryptionService = encryptionService;
            _authenticationService = authenticationService;
            _ewayBillUrl = ewayBillUrl;
            _configuration = configuration;
        }

        private TokenInfo GetSession()
        {
            TokenInfo token = _authenticationService.GetToken();
            if (token == null)
            {
                throw new Exception("Authentication token expired. Please authenticate again.");
            }
            return token;
        }

        /// <summary>
        /// Generate E-Way Bill.
        /// </summary>
        public async Task<GenerateEWayBillResponse> GenerateEWayBillAsync(GenerateEWayBillRequest request)
        {
            TokenInfo token = GetSession();
            string encryptedPayload = _encryptionService.EncryptObject(request, token.Sek);
            string response = await _httpService.PostRawAsync(_ewayBillUrl, "GENEWAYBILL", encryptedPayload, token.AuthToken, _configuration);
            GenerateEWayBillResponse result = JsonHelper.Deserialize<GenerateEWayBillResponse>(response);
            return result;
        }

        /// <summary>
        /// Get E-Way Bill details.
        /// </summary>
        public async Task<GetEWayBillResponse> GetEWayBillAsync(GetEWayBillRequest request)
        {
            TokenInfo token = GetSession();
            string encryptedPayload = _encryptionService.EncryptObject(request, token.Sek);
            string response = await _httpService.PostRawAsync(_ewayBillUrl, "", encryptedPayload, token.AuthToken, _configuration);

            return _encryptionService.DecryptObject<GetEWayBillResponse>(response, token.Sek);
        }

        /// <summary>
        /// Cancel E-Way Bill.
        /// </summary>
        public async Task<CancelEWayBillResponse> CancelEWayBillAsync(CancelEWayBillRequest request)
        {
            TokenInfo token = GetSession();
            string encryptedPayload = _encryptionService.EncryptObject(request, token.Sek);
            string response = await _httpService.PostRawAsync(_ewayBillUrl, "CANEWB", encryptedPayload, token.AuthToken, _configuration);
            CancelEWayBillResponse retusl = JsonHelper.Deserialize<CancelEWayBillResponse>(response);
            return retusl;
        }

        /// <summary>
        /// Update vehicle details.
        /// </summary>
        public async Task<UpdateVehicleResponse> UpdateVehicleAsync(UpdateVehicleRequest request)
        {
            TokenInfo token = GetSession();
            string encryptedPayload = _encryptionService.EncryptObject(request, token.Sek);
            string response = await _httpService.PostRawAsync(_ewayBillUrl, "", encryptedPayload, token.AuthToken, _configuration);
            return _encryptionService.DecryptObject<UpdateVehicleResponse>(response, token.Sek);
        }
        public async Task<string> GetEWayBill(long ewbNo)
        {
            TokenInfo token = GetSession();
            string queryString = "GetEwayBill&ewbNo=" + ewbNo;
            string response = await _httpService.GetRawAsync(_ewayBillUrl, queryString, token.AuthToken, _configuration);
            return response;
        }
        public async Task<byte[]> PrintEWayBill(string ewayBillJson)
        {
            TokenInfo token = GetSession();

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("aspid", _configuration.EWayBill_AspId);
                client.DefaultRequestHeaders.Add("password", _configuration.EWayBill_AspPassword);
                client.DefaultRequestHeaders.Add("Gstin", _configuration.GSTIN);
                client.DefaultRequestHeaders.Add("username", _configuration.EWayBill_EWBUsername);
                client.DefaultRequestHeaders.Add("ewbpwd", _configuration.EWayBill_EWBPassword);
                client.DefaultRequestHeaders.Add("authtoken", token.AuthToken);
                var content = new StringContent(ewayBillJson, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://einvapi.charteredinfo.com/aspapi/v1.0/printdetailewb", content);

                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();

                    throw new Exception(
                        $"Status Code: {(int)response.StatusCode} ({response.StatusCode})\n" +
                        $"Response: {error}");
                }

                return await response.Content.ReadAsByteArrayAsync();
            }
        }
        /// <summary>
        /// Extend validity of E-Way Bill.
        /// </summary>
        public async Task<ExtendValidityResponse> ExtendValidityAsync(ExtendValidityRequest request)
        {
            TokenInfo token = GetSession();
            string encryptedPayload = _encryptionService.EncryptObject(request, token.Sek);
            string response = await _httpService.PostRawAsync(_ewayBillUrl, "", encryptedPayload, token.AuthToken, _configuration);
            return _encryptionService.DecryptObject<ExtendValidityResponse>(response, token.Sek);
        }
    }
}