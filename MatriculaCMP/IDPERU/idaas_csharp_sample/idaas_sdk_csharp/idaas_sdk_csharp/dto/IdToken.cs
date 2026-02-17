using Newtonsoft.Json;
using System;
using System.Collections.Generic;

/**
 * @author David PAXI
 */
namespace idaas_sdk_csharp.dto
{
    public class IdToken
    {
        [JsonProperty("acr")]
        public String acr { get; set; }
        [JsonProperty("aud")]
        public List<String> aud { get; set; }
        [JsonProperty("sub")]
        public String sub { get; set; }
        [JsonProperty("doc")]
        public String doc { get; set; }
        [JsonProperty("first_name")]
        public String firstName { get; set; }
        [JsonProperty("email")]
        public String email { get; set; }
        [JsonProperty("email_verified")]
        public Boolean emailVerified { get; set; }
        [JsonProperty("phone_number")]
        public String phoneNumber { get; set; }
        [JsonProperty("phone_number_verified")]
        public Boolean phoneNumberVerified { get; set; }
        [JsonProperty("exp")]
        public Int32 exp { get; set; }
        [JsonProperty("iat")]
        public Int32 iat { get; set; }
        [JsonProperty("iss")]
        public String iss { get; set; }
        [JsonProperty("nonce")]
        public String nonce { get; set; }
        [JsonProperty("at_hash")]
        public String athash { get; set; }
        [JsonProperty("c_hash")]
        public String chash { get; set; }
    }
}
