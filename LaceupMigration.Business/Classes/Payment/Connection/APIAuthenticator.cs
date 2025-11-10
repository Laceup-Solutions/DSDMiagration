using RestSharp.Authenticators;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace LaceupMigration
{
    public class APIAuthenticator : AuthenticatorBase
    {
        public string _baseUrl = Config.UseProductionForPayments ? "https://webpay.agilpay.net/" : "https://sandbox-webapi.agilpay.net";
        public string _clientId = CompanyInfo.GetMasterClientId();
        public string _clientSecret = CompanyInfo.GetMasterClientSecretId();

        public APIAuthenticator(string baseUrl, string clientId, string clientSecret) : base("")
        {
            _baseUrl = baseUrl;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
        {
            var token = string.IsNullOrEmpty(Token) ? await GetToken() : Token;
            return new HeaderParameter(KnownHeaders.Authorization, token);
        }

        public async Task<string> GetToken()
        {
            try
            {
                var options = new RestClientOptions(_baseUrl);
                var client = new RestClient(options);
                var request = new RestRequest("oauth/token", Method.Post);

                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("Client_id", _clientId);
                request.AddParameter("Client_Secret", _clientSecret);
                request.AddParameter("grant_type", "client_credentials");

                RestResponse response = await client.ExecuteAsync(request);

                System.Console.WriteLine(response.Content);

                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var tokenResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    return $"{tokenResponse.TokenType} {tokenResponse.AccessToken}";
                }

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            return null;
        }
    }
}