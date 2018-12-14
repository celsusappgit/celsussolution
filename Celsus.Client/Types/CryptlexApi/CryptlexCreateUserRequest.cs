using Newtonsoft.Json;

namespace Celsus.Client.Types.CryptlexApi
{
    public class CryptlexCreateUserRequest
    {
        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("metadata")]
        public CryptlexMetadata[] Metadata { get; set; }

        public static CryptlexCreateUserRequest FromJson(string json) => JsonConvert.DeserializeObject<CryptlexCreateUserRequest>(json, CryptlexConverter.Settings);
    }


}
