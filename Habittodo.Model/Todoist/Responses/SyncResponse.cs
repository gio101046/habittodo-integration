using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Habittodo.Model.Todoist.Responses
{
    public class SyncResponse
    {
        [JsonProperty("sync_token")]
        public string Sync_token { get; set; }
        [JsonProperty("full_sync")]
        public bool Full_sync { get; set; }
        [JsonProperty("items")]
        public List<Item> Items { get; set; }
    }
}
