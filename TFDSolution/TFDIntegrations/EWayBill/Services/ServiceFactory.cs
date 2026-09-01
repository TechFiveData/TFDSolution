using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Transport.Integration;

namespace TFDIntegrations.EWayBill.Services
{
    public static class ServiceFactory
    {
        public static AuthenticationService CreateAuthenticationService(IntegrationConfiguration configuration)
        {
            HttpService httpService = new HttpService();
            return new AuthenticationService(
                httpService,
                ConfigurationManager.AppSettings["EWayBill.AuthUrl"],
                configuration
            );
        }
        public static EWayBillService CreateEWayBillService(IntegrationConfiguration configuration)
        {
            HttpService httpService = new HttpService();
            EncryptionService encryptionService = new EncryptionService();
            AuthenticationService authenticationService = CreateAuthenticationService(configuration);
            return new EWayBillService(httpService, encryptionService, authenticationService, ConfigurationManager.AppSettings["EWayBill.ApiUrl"], configuration);
        }
    }
}