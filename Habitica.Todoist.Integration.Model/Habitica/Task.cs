using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Habitica.Todoist.Integration.Model.Habitica
{
    public class Task
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("date")]
        public string Date { get; set; }
        [JsonProperty("priority")]
        public string Priority { get; set; }
    }
}
