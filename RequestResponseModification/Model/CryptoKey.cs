using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace RequestResponseModification.Model
{
    public class CryptoKey
    {
        [Newtonsoft.Json.JsonProperty("id")]
        public string Id { get; set; }

        [Newtonsoft.Json.JsonProperty("publlicKey")]
        public bool PublicKey { get; set; }

        [Newtonsoft.Json.JsonProperty("privateKey")]
        public bool PrivateKey { get; set; }

        [Newtonsoft.Json.JsonProperty("key")]
        public RSAParameters Key { get; set; }

        [Newtonsoft.Json.JsonProperty("side")]
        public string Side { get; set; }
    }
}
