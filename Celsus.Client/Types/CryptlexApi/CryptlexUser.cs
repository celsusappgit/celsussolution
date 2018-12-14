using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Celsus.Client.Types.CryptlexApi
{

    public class CryptlexUser
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("twoFactorEnabled")]
        public bool TwoFactorEnabled { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("roles")]
        public List<object> Roles { get; set; }

        [JsonProperty("lastLoginAt")]
        public DateTimeOffset? LastLoginAt { get; set; }

        [JsonProperty("lastSeenAt")]
        public DateTimeOffset? LastSeenAt { get; set; }

        [JsonProperty("metadata")]
        public List<object> Metadata { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        public static List<CryptlexUser> FromJson(string json) => JsonConvert.DeserializeObject<List<CryptlexUser>>(json, CryptlexConverter.Settings);
    }







}
