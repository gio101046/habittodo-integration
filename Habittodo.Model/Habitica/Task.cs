using Newtonsoft.Json;
using System.Collections.Generic;

namespace Habittodo.Model.Habitica
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
        [JsonProperty("checklist", NullValueHandling = NullValueHandling.Ignore)]
        public List<ChecklistItem> Checklist { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
