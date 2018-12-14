using Newtonsoft.Json;
using System.Collections.Generic;

namespace Celsus.Client.Types.CryptlexApi
{
    public static class CryptlexSerialize
    {
        public static string ToJson(this List<CryptlexUser> self) => JsonConvert.SerializeObject(self, CryptlexConverter.Settings);
        public static string ToJson(this CryptlexCreateUserRequest self) => JsonConvert.SerializeObject(self, CryptlexConverter.Settings);
        public static string ToJson(this CryptlexUser self) => JsonConvert.SerializeObject(self, CryptlexConverter.Settings);

    }

}
