using Newtonsoft.Json;

namespace Celsus.Client.Types.CryptlexApi
{
    public partial class CryptlexMetadata
    {
        [JsonProperty("visible")]
        public bool Visible { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

}
