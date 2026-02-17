using IDPeru.common;
using IDPeru.dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;



namespace IDPeru
{
    public class ReniecIdaasClient
    {
        public String redirectUri { get; set; }
        public List<String> lstScopes { get; set; }
        public String acr { get; set; }
        public String prompt { get; set; }
        public Int32 maxAge { get; set; }
        public String state { get; set; }

        public String vd { get; set; }
        private String loginHint { get; set; }
        private Config config { get; set; }

        public ReniecIdaasClient(String configFile)
        {
            redirectUri = null;
            lstScopes = new List<String>();
            acr = Constants.ACR_ONE_FACTOR;
            prompt = null;
            maxAge = -1;
            state = null;
            loginHint = null;

            using (StreamReader file = File.OpenText(@configFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                config = (Config)serializer.Deserialize(file, typeof(Config));
            }
        }

        public String getLoginUrl()
        {
            String paramScope = "openid";

            Dictionary<String, String> query = new Dictionary<String, String>
            {
                { "acr_values", acr },
                { "client_id", config.clientId },
                { "response_type", "code" },
                { "redirect_uri", redirectUri }
            };


            if (prompt != null)
            {
                query.Add("prompt", prompt);
            }
            
            if (state != null)
            {
                query.Add("state", state);
            }

            if (maxAge > -1)
            {
                query.Add("max_age", maxAge.ToString());
            }
           
            if (vd != null)
            {
                byte[] key = Encoding.UTF8.GetBytes(config.clientId.Substring(0,16)); 
                byte[] iv = Encoding.UTF8.GetBytes(config.clientId.Substring(0,16));
                query.Add("vd", encryptdni.Encrypt(this.vd,key,iv));
            }
            
            if (loginHint != null)
            {
                query.Add("login_hint", loginHint);
            }

            //lstScopes
            foreach (String scope in lstScopes)
            {
                paramScope += " " + scope;
            }

            query["scope"] = paramScope;


            //Bulding query params
            StringBuilder sb = new StringBuilder();
            sb.Append(config.authUri + "?");

            foreach (var item in query)
            {
                sb.Append(string.Format("{0}={1}&", item.Key, UrlHelper.Encode(item.Value)));
            }

            String url = sb.ToString();
            url = url.Remove(url.LastIndexOf("&"));

            return url;
        }

        public async Task<User> getUserInfo(String accessToken)
        {
            try
            {
                System.Net.ServicePointManager.Expect100Continue = false;

                HttpClient myHttpClient = new HttpClient();
                myHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await myHttpClient.PostAsync(config.userInfoUri, null);

                if (response.IsSuccessStatusCode)
                {
                    String content = await response.Content.ReadAsStringAsync();

                    Console.WriteLine(content);
                    User oUser = JsonConvert.DeserializeObject<User>(content);

                    return oUser;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }

            return null;
        }

        public async Task<TokenResponse> getTokens(String code)
        {
            try
            {
                System.Net.ServicePointManager.Expect100Continue = false;

                FormUrlEncodedContent formContent = new FormUrlEncodedContent(new[]{
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("client_id", config.clientId),
                    new KeyValuePair<string, string>("client_secret",config.clientSecret),
                });

                HttpClient myHttpClient = new HttpClient();
                var response = await myHttpClient.PostAsync(config.tokenUri, formContent);

                if (response.IsSuccessStatusCode)
                {
                    String content = await response.Content.ReadAsStringAsync();
                    TokenResponse tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(content);
                    //String[] parts = tokenResponse.idToken.Split('.');
                    //String payload = Utils.Base64Decode(parts[1]);

                    //Console.WriteLine(payload);

                    //tokenResponse.idTokenObject = JsonConvert.DeserializeObject<IdToken>(payload);

                    return tokenResponse;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }

            return null;
        }

        public String getLogoutUri(String redirectPostLogout)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(config.logoutUri + "?");
            sb.Append(string.Format("{0}={1}", "post_logout_redirect_uri", UrlHelper.Encode(redirectPostLogout)));

            String url = sb.ToString();

            return url;
        }
    }

  
}
