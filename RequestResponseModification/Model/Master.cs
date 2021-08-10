using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RequestResponseModification.Model
{
    public class Master
    {
        [Newtonsoft.Json.JsonProperty("id")]
        public string Id { get; set; }

        [Newtonsoft.Json.JsonProperty("username")]
        public string Username { get; set; }

        [Newtonsoft.Json.JsonProperty("password")]
        public string Password { get; set; }

        [Newtonsoft.Json.JsonProperty("email")]
        public string Email { get; set; }
    }
}
