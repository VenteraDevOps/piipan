﻿using Newtonsoft.Json;

namespace Piipan.States.Api.Models
{
    public class StateInfoResponseData : IState
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("state_abbreviation")]
        public string StateAbbreviation { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }
    }
}
