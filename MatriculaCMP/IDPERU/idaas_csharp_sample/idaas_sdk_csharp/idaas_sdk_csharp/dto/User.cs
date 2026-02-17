using Newtonsoft.Json;
using System;

/**
 * @author David PAXI
 */
namespace idaas_sdk_csharp.dto
{
    public class User
    {
        [JsonProperty("doc")]
        public String doc { get; set; }
        [JsonProperty("first_name")]
        public String firstName { get; set; }
        [JsonProperty("phone_number")]
        public String phoneNumber { get; set; }
        [JsonProperty("phone_number_verified")]
        public Boolean phoneNumberVerified { get; set; }
        [JsonProperty("email")]
        public String email { get; set; }
        [JsonProperty("email_verified")]
        public Boolean emailVerified { get; set; }
        [JsonProperty("sub")]
        public String sub { get; set; }
    }
}
