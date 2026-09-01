using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TFDIntegrations.EInvoiceBill.Model;
using TFDIntegrations.EInvoiceBill.Model.Request;
using TFDIntegrations.EWayBill.Models;
using TFDIntegrations.EWayBill.Services;
using TFDSolution.Transport.Integration;

namespace TFDIntegrations.EInvoiceBill.Services
{
    public class IRNService
    {
        private readonly NICAuthenticationService _authenticationService;
        private readonly IntegrationConfiguration _configuration;
        private string ApiUrl = ConfigurationManager.AppSettings["EInvoiceBill.ApiUrl"];
        public IRNService(NICAuthenticationService authenticationService, IntegrationConfiguration configuration)
        {
            _authenticationService = authenticationService;
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
        public async Task<GenerateIRNResponse> GenerateIRNNew(EInvoiceRequest invoice, string authToken)
        {
            string json = JsonConvert.SerializeObject(invoice);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();

                client.DefaultRequestHeaders.Add("Gstin", _configuration.GSTIN);
                client.DefaultRequestHeaders.Add("user_name", _configuration.EWayBill_EWBUsername);
                client.DefaultRequestHeaders.Add("AuthToken", authToken);

                client.DefaultRequestHeaders.Add("aspid", _configuration.EWayBill_AspId);
                client.DefaultRequestHeaders.Add("password", _configuration.EWayBill_AspPassword);

                // 1 = NIC IRP
                // 2 = IRIS IRP
                client.DefaultRequestHeaders.Add("irpurl", "1");

                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage httpResponse = await client.PostAsync($"{ApiUrl}/Invoice?QrCodeSize=250", content);

                string responseJson = await httpResponse.Content.ReadAsStringAsync();

                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"HTTP {(int)httpResponse.StatusCode}: {responseJson}");
                }

                EInvoiceErrorResponse response = JsonConvert.DeserializeObject<EInvoiceErrorResponse>(responseJson);
                GenerateIRNResponse generateIRNResponse = new GenerateIRNResponse();
                if (response == null)
                    throw new Exception("Empty response received.");

                if (response.Status != "1")
                {
                    if (response.ErrorDetails != null)
                        throw new Exception($"{response.ErrorDetails[0].ErrorCode} : {response.ErrorDetails[0].ErrorMessage}");

                    throw new Exception("IRN generation failed.");
                }
                else
                {
                    generateIRNResponse = JsonConvert.DeserializeObject<GenerateIRNResponse>(response.Data);
                }
                generateIRNResponse.ResponseJson = responseJson;
                return generateIRNResponse;
            }
        }
        public async Task<GenerateIRNResponse> GenerateIRN(EInvoiceRequest invoice, string authToken)
        {
            //TokenInfo token = GetSession();
            string json = JsonConvert.SerializeObject(invoice, Formatting.None);
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", authToken);
                client.DefaultRequestHeaders.Add("user_name", _configuration.EWayBill_EWBUsername);
                client.DefaultRequestHeaders.Add("gstin", _configuration.GSTIN);
                client.DefaultRequestHeaders.Add("aspid", _configuration.EWayBill_AspId);
                client.DefaultRequestHeaders.Add("password", _configuration.EWayBill_AspPassword);

                var response = await client.PostAsync(ApiUrl + "/Invoice?QrCodeSize=250", new StringContent(json, Encoding.UTF8, "application/json"));
                string result = await response.Content.ReadAsStringAsync();

                GenerateIRNResponse irnResponse = JsonConvert.DeserializeObject<GenerateIRNResponse>(result);
                return irnResponse;
            }
        }
        public async Task<DownloadSignedInvoiceResponse> DownloadSignedInvoice(string irn, string _authToken, string folder)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();

                client.DefaultRequestHeaders.Add("aspid", _configuration.EWayBill_AspId);
                client.DefaultRequestHeaders.Add("password", _configuration.EWayBill_AspPassword);
                client.DefaultRequestHeaders.Add("Gstin", _configuration.GSTIN);
                client.DefaultRequestHeaders.Add("user_name", _configuration.EWayBill_EWBUsername);
                client.DefaultRequestHeaders.Add("AuthToken", _authToken);

                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                string url = ApiUrl + $"/DownloadSignedInvoice?Irn={Uri.EscapeDataString(irn)}";

                HttpResponseMessage response = await client.GetAsync(url);
                byte[] data = await response.Content.ReadAsByteArrayAsync();

                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    throw new Exception(error);
                }

                byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

                if (fileBytes == null || fileBytes.Length == 0)
                    throw new Exception("No file received from API.");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string filePath = Path.Combine(folder, $"eInvoiceBill_{irn}.pdf");

                File.WriteAllBytes(filePath, fileBytes);

                DownloadSignedInvoiceResponse response1 = new DownloadSignedInvoiceResponse();
                response1.Status = 1;
                return response1;
                //Console.WriteLine($"Bytes received: {data.Length}");
                //string json = await response.Content.ReadAsStringAsync();

                //if (!response.IsSuccessStatusCode)
                //    throw new Exception(json);

                //return JsonConvert.DeserializeObject<DownloadSignedInvoiceResponse>(json);
            }
        }
        public async Task<byte[]> PrintEInvoiceBill(string irnResponse, string authToken)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("aspid", _configuration.EWayBill_AspId);
                client.DefaultRequestHeaders.Add("password", _configuration.EWayBill_AspPassword);
                client.DefaultRequestHeaders.Add("Gstin", _configuration.GSTIN);
                client.DefaultRequestHeaders.Add("username", _configuration.EWayBill_EWBUsername);
                client.DefaultRequestHeaders.Add("ewbpwd", _configuration.EWayBill_EWBPassword);
                client.DefaultRequestHeaders.Add("authtoken", authToken);
                var content = new StringContent(irnResponse, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://einvapi.charteredinfo.com/aspapi/v1.0/printinvoice", content);

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
        public async Task<CancelIRNResponse> CancelInvoice(CancelInvoiceRequest request, string authToken)
        {
            string json = JsonConvert.SerializeObject(request);
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();

                client.DefaultRequestHeaders.Add("Gstin", _configuration.GSTIN);
                client.DefaultRequestHeaders.Add("user_name", _configuration.EWayBill_EWBUsername);
                client.DefaultRequestHeaders.Add("AuthToken", authToken);

                client.DefaultRequestHeaders.Add("aspid", _configuration.EWayBill_AspId);
                client.DefaultRequestHeaders.Add("password", _configuration.EWayBill_AspPassword);

                // 1 = NIC IRP
                // 2 = IRIS IRP
                client.DefaultRequestHeaders.Add("irpurl", "1");
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = await client.PostAsync($"{ApiUrl}/Invoice/Cancel", content);
                string responseJson = await httpResponse.Content.ReadAsStringAsync();
                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"HTTP {(int)httpResponse.StatusCode}: {responseJson}");
                }

                EInvoiceErrorResponse response = JsonConvert.DeserializeObject<EInvoiceErrorResponse>(responseJson);
                CancelIRNResponse generateIRNResponse = new CancelIRNResponse();
                if (response == null)
                    throw new Exception("Empty response received.");

                if (response.Status != "1")
                {
                    if (response.ErrorDetails != null)
                        throw new Exception($"{response.ErrorDetails[0].ErrorCode} : {response.ErrorDetails[0].ErrorMessage}");

                    throw new Exception("IRN generation failed.");
                }
                else
                {
                    generateIRNResponse = JsonConvert.DeserializeObject<CancelIRNResponse>(response.Data);
                }
                if (generateIRNResponse != null && !string.IsNullOrEmpty(generateIRNResponse.CancelDate))
                {
                    generateIRNResponse.Status = "1";
                }
                return generateIRNResponse;
            }
        }
        public async Task<string> GetIRNDetails(string authToken, string irn)
        {
            string responseJson = string.Empty;
            var request = new
            {
                Irn = irn
            };
            string json = JsonConvert.SerializeObject(request);
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", authToken);
                client.DefaultRequestHeaders.Add("user_name", _configuration.EWayBill_EWBUsername);
                client.DefaultRequestHeaders.Add("gstin", _configuration.GSTIN);
                client.DefaultRequestHeaders.Add("aspid", _configuration.EWayBill_AspId);
                client.DefaultRequestHeaders.Add("password", _configuration.EWayBill_AspPassword);

                var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );
                var response = await client.PostAsync(ApiUrl + "/GetInvoiceDetails",content);

                responseJson = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(
                        $"IRN Details API failed: {response.StatusCode} - {responseJson}"
                    );
                }
            }

            return responseJson;
        }
    }
}