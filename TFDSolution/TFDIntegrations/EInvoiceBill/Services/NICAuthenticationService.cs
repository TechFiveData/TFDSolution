using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TFDIntegrations.EInvoiceBill.Model;
using TFDIntegrations.EWayBill.Helpers;
using TFDIntegrations.EWayBill.Models;
using TFDIntegrations.EWayBill.Services;
using TFDSolution.Transport.Integration;

namespace TFDIntegrations.EInvoiceBill.Services
{
    public class NICAuthenticationService
    {
        private readonly HttpService _httpService;
        private readonly string _authenticationUrl;
        private readonly string _gstin;
        private static TokenInfo _tokenInfo;
        private static IntegrationConfiguration _configuration;
        public NICAuthenticationService(IntegrationConfiguration configuration)
        {
            _configuration = configuration;
            _authenticationUrl = ConfigurationManager.AppSettings["EInvoiceBill.AuthUrl"];
        }
        public async Task<TokenInfo> AuthenticateAsyncNew()
        {
            try
            {
                string aspId = ConfigurationManager.AppSettings["Integration.AspId"];
                string aspPassword = ConfigurationManager.AppSettings["Integration.AspPassword"];

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();

                    client.DefaultRequestHeaders.Add("aspid", aspId);
                    client.DefaultRequestHeaders.Add("password", aspPassword);
                    client.DefaultRequestHeaders.Add("Gstin", _configuration.GSTIN);
                    client.DefaultRequestHeaders.Add("user_name", _configuration.EWayBill_EWBUsername);
                    client.DefaultRequestHeaders.Add("eInvPwd", _configuration.EWayBill_EWBPassword);

                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage httpResponse = await client.GetAsync(_authenticationUrl);

                    string responseJson = await httpResponse.Content.ReadAsStringAsync();

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        throw new Exception($"HTTP {(int)httpResponse.StatusCode}: {responseJson}");
                    }
                    AuthenticationResponse response = JsonConvert.DeserializeObject<AuthenticationResponse>(responseJson);
                    if (response == null)
                        throw new Exception("Authentication response is empty.");

                    // Handle API error response
                    if (response.Status != 1)
                    {
                        //ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseJson);

                        //if (errorResponse != null && errorResponse.error != null)
                        //{
                        //    throw new Exception($"{errorResponse.error.error_cd} : {errorResponse.error.message}");
                        //}
                        if (response.ErrorDetails != null)
                            throw new Exception($"{response.ErrorDetails.ErrorCode} : {response.ErrorDetails.ErrorMessage}");

                        throw new Exception("Authentication failed.");
                    }

                    _tokenInfo = new TokenInfo
                    {
                        Gstin = _configuration.GSTIN,
                        Username = _configuration.EWayBill_EWBUsername,
                        AuthToken = response.Data.AuthToken,
                        Sek = response.Data.Sek,
                        CreatedOn = DateTime.Now,
                        ExpiryOn = DateTime.Now.AddHours(6)
                    };

                    return _tokenInfo;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Authentication failed: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Returns current token.
        /// Re-authentication can be triggered when expired.
        /// </summary>
        public TokenInfo GetToken()
        {
            if (_tokenInfo == null)
                return null;

            if (_tokenInfo.IsExpired())
                return null;

            return _tokenInfo;
        }

        /// <summary>
        /// Clears current session.
        /// </summary>
        public void Logout()
        {
            _tokenInfo = null;
        }
    }
}