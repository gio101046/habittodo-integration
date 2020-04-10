using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Habitica.Todoist.Integration.Model.Habitica
{
    public class ChecklistItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
