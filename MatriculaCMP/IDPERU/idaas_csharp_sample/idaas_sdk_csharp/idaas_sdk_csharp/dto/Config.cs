using Newtonsoft.Json;
using System;
using System.Collections.Generic;

/**
 * @author David PAXI
 */
namespace idaas_sdk_csharp.dto
{
    public class Config
    {
        [JsonProperty("client_id")]
        public String clientId { get; set; }
        [JsonProperty("client_secret")]
        public String clientSecret { get; set; }
        [JsonProperty("auth_uri")]
        public String authUri { get; set; }
        [JsonProperty("token_uri")]
        public String tokenUri { get; set; }
        [JsonProperty("userinfo_uri")]
        public String userInfoUri { get; set; }
        [JsonProperty("logout_uri")]
        public String logoutUri { get; set; }
        [JsonProperty("auth_provider_keys_uri")]
        public String keysUri { get; set; }
        [JsonProperty("redirect_uris")]
        public List<String> redirectUris { get; set; }
        [JsonProperty("javascript_origins")]
        public List<String> originUris { get; set; }
    }
}
