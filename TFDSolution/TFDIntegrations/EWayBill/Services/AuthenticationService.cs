using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TFDIntegrations.EWayBill.Helpers;
using TFDIntegrations.EWayBill.Models;
using TFDSolution.Transport.Integration;

namespace TFDIntegrations.EWayBill.Services
{
    /// <summary>
    /// Handles NIC E-Way Bill authentication.
    /// </summary>
    public class AuthenticationService
    {
        private readonly HttpService _httpService;
        private readonly string _authenticationUrl;        
        private static TokenInfo _tokenInfo;
        private static IntegrationConfiguration _configuration;

        public AuthenticationService(HttpService httpService, string authenticationUrl,IntegrationConfiguration configuration)
        {
            _httpService = httpService;
            _authenticationUrl = authenticationUrl;            
            _configuration = configuration;
        }
        public async Task<TokenInfo> AuthenticateAsync()
        {
            try
            {
                string url = ConfigurationManager.AppSettings["EWayBill.AuthUrl"];

                string aspId = ConfigurationManager.AppSettings["Integration.AspId"];
                string aspPassword = ConfigurationManager.AppSettings["Integration.AspPassword"];

                string username = _configuration.EWayBill_EWBUsername;
                string ewbPassword = _configuration.EWayBill_EWBPassword;
                string gstin = _configuration.GSTIN;

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("aspid", aspId);
                    client.DefaultRequestHeaders.Add("password", aspPassword);
                    client.DefaultRequestHeaders.Add("Gstin", gstin);
                    client.DefaultRequestHeaders.Add("username", username);
                    client.DefaultRequestHeaders.Add("ewbpwd", ewbPassword);

                    HttpResponseMessage httpResponse = await client.GetAsync(url);

                    string responseJson = await httpResponse.Content.ReadAsStringAsync();

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        throw new Exception($"HTTP {(int)httpResponse.StatusCode}: {responseJson}");
                    }
                    AuthTokenResponse response = JsonConvert.DeserializeObject<AuthTokenResponse>(responseJson);
                    if (response == null)
                        throw new Exception("Authentication response is empty.");

                    if (response.Status != "1" && response.ErrorDetails != null)
                        throw new Exception(response.ErrorDetails.ErrorMessage);


                    _tokenInfo = new TokenInfo
                    {
                        Gstin = gstin,
                        Username = username,
                        AuthToken = response.AuthToken,
                        Sek = response.Sek,
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