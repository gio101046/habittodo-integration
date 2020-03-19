using Newtonsoft.Json;
using System;
using System.Linq;

namespace Habitica.Todoist.Integration.Model.Todoist
{
    public class Item
    {
        [JsonProperty("content")]
        public string Content { get; set; }
        [JsonProperty("Id")]
        public string Id { get; set; }
        [JsonProperty("due")]
        public Due Due { get; set; }
        [JsonProperty("is_deleted")]
        public int Is_deleted { get; set; }
        [JsonProperty("date_completed")]
        public string Date_completed { get; set; }

        public int? GetDifficulty()
        {
            try { return int.Parse(Content.Split('-').Last().Last().ToString()); } catch { }
            return null;
        }

        public string GetCleanContent()
        {
            try { return Content.Split('-').First(); } catch { }
            return null;
        }
    }
}
