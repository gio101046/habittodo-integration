using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Habitica.Todoist.Integration.Model.Habitica.Responses
{
    public class HabiticaReponse<T>
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("data")]
        public T Data { get; set; }
        [JsonProperty("notifications")]
        public object Notifications { get; set; }
    }
}
