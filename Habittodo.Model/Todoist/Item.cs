using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Habittodo.Model.Todoist
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
        [JsonProperty("parent_id")]
        public string Parent_Id { get; set; }
        [JsonProperty("is_deleted")]
        public int Is_deleted { get; set; }
        [JsonProperty("project_id")]
        public string Project_id { get; set; }
        [JsonProperty("date_completed")]
        public string Date_completed { get; set; }

        /// <summary>
        /// This field will always be false when an item is marked as deleted
        /// </summary>
        [JsonIgnore]
        public bool IsChild => !string.IsNullOrEmpty(Parent_Id);
    }
}
