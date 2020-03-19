using Newtonsoft.Json;
using System;
using System.Linq;

namespace Habitica.Todoist.Integration.Model.Todoist
{
    public class Item
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("content")]
        public string Content { get; set; }
        [JsonProperty("due")]
        public Due Due { get; set; }
        [JsonProperty("priority")]
        public int Priority { get; set; }
        [JsonProperty("is_deleted")]
        public int Is_deleted { get; set; }
        [JsonProperty("date_completed")]
        public string Date_completed { get; set; }
    }
}
