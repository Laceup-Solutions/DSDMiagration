





using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaceupMigration
{
    internal class ApiClient : IBaseApiClient
    {
        private readonly RestClient _client;

        // public string CompanyIdKey = "API-001";
        //public string CompanyNameKey = "Dynapay";
        public string CompanyIdKey = CompanyInfo.GetMasterClientId();
        public string CompanyNameKey = CompanyInfo.GetMasterClientSecretId();

        public string url = Config.UseProductionForPayments ? "https://webapi.agilpay.net/" : "https://sandbox-webapi.agilpay.net";
        public ApiClient()
        {
            var options = new RestClientOptions(url)
            {
                Authenticator = new APIAuthenticator(url, CompanyIdKey, CompanyNameKey)
            };

            _client = new RestClient(options);
        }

        public async Task<PaymentResponse> Authorize(PaymentRequest paymentRequest)
        {
            PaymentResponse rest = null;

            try
            {
                var request = new RestRequest("/Payment6/Autorize", Method.Post);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("SessionId", "ABCDEF");
                request.AddHeader("SiteId", CompanyIdKey);
                request.AddJsonBody(paymentRequest);

                string jsonString = JsonConvert.SerializeObject(paymentRequest);

                RestResponse response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful && response.Content != null)
                {
                    rest = JsonConvert.DeserializeObject<PaymentResponse>(response.Content);
                    return rest;
                }
                else
                {
                    rest = JsonConvert.DeserializeObject<PaymentResponse>(response.Content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return rest;
        }

        public async Task<PaymentResponse> AuthorizeToken(PaymentAtutorizeToken paymentAtutorizeToken)
        {
            PaymentResponse rest = null;

            try
            {
                var request = new RestRequest("/Payment5/AutorizeToken", Method.Post);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("SessionId", "ABCDEF");
                request.AddHeader("SiteId", CompanyIdKey);
                request.AddJsonBody(paymentAtutorizeToken);

                RestResponse response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful && response.Content != null)
                {
                    rest = JsonConvert.DeserializeObject<PaymentResponse>(response.Content);
                    return rest;
                }
                else
                {
                    rest = JsonConvert.DeserializeObject<PaymentResponse>(response.Content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return rest;
        }
    }
}