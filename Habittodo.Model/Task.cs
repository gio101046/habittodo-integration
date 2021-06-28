using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Habittodo.Model
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
