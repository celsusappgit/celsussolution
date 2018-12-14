using Newtonsoft.Json;

namespace Celsus.Client.Types.CryptlexApi
{
    public  class CryptlexError
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        public static CryptlexError FromJson(string json) => JsonConvert.DeserializeObject<CryptlexError>(json, CryptlexConverter.Settings);
    }

}
