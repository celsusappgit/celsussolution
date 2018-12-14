using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;

namespace Celsus.Client.Types.CryptlexApi
{
    internal static class CryptlexConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

}
