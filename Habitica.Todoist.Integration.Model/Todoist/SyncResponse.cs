using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Habitica.Todoist.Integration.Model.Todoist
{
    public class SyncResponse
    {
        [JsonProperty("sync_token")]
        public string Sync_token { get; set; }
        [JsonProperty("full_sync")]
        public bool Full_sync { get; set; }
        [JsonProperty("itmes")]
        public List<Item> Items { get; set; }
    }
}
