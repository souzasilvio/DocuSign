using DocuSign.eSign.Client;
using DocuSign.eSign.Client.Auth;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Text;
using static DocuSign.eSign.Client.Auth.OAuth.UserInfo;

namespace AppWebHook.Service
{
    public class ServiceApi : IServiceApi
    {
        private IConfiguration Config;
        private ApiClient _apiClient;
        
        public ApiClient ApiClient {
            get{
                CheckToken();
                return _apiClient;
            }

        }

        public ServiceApi(IConfiguration config)
        {
            Config = config;
            _apiClient = new ApiClient(config.GetValue<string>("DocuSign:Host"));
        }
        private const int TOKEN_REPLACEMENT_IN_SECONDS = 10 * 60;

        private static string AccessToken { get; set; }
        private static int expiresIn;
        private static Account Account { get; set; }

        

        public string AccountID
        {
            get { return Account.AccountId; }
        }


        private void CheckToken()
        {
            if (AccessToken == null
                || (DateTime.Now.Millisecond + TOKEN_REPLACEMENT_IN_SECONDS) > expiresIn)
            {
                UpdateToken();
            }
        }

        private void UpdateToken()
        {
            var userId = Config.GetValue<string>("DocuSign:UserId");            
            var authServer = Config.GetValue<string>("DocuSign:OAuthBasePath");
            var clientId = Config.GetValue<string>("DocuSign:ClientId");
            var privateKeyFilename = Config.GetValue<string>("DocuSign:PrivateKeyFilename");
            //Encoding.UTF8.GetBytes(DSConfig.PrivateKey),
            var privateKey = Encoding.UTF8.GetBytes(File.ReadAllText(privateKeyFilename));
            //var privateKey = File.ReadAllText(privateKeyFilename);
            OAuth.OAuthToken authToken = _apiClient.RequestJWTUserToken(clientId,
                            userId,
                            authServer,
                            privateKey,
                            1);

            AccessToken = authToken.access_token;

            if (Account == null)
                Account = GetAccountInfo(authToken);

            _apiClient = new ApiClient(Account.BaseUri + "/restapi");            
            expiresIn = DateTime.Now.Second + authToken.expires_in.Value;
        }

        private Account GetAccountInfo(OAuth.OAuthToken authToken)
        {
            var authServer = Config.GetValue<string>("DocuSign:OAuthBasePath");
            _apiClient.SetOAuthBasePath(authServer);
            OAuth.UserInfo userInfo = _apiClient.GetUserInfo(authToken.access_token);
            Account acct = null;

            var accounts = userInfo.Accounts;
            acct = accounts.FirstOrDefault(a => a.IsDefault == "true");           

            return acct;
        }
    }
}
