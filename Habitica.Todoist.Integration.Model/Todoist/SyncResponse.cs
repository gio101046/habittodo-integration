using System;
using System.Collections.Generic;
using System.Text;

namespace Habitica.Todoist.Integration.Model.Todoist
{
    public class SyncResponse
    {
        public string Sync_token { get; set; }
        public bool Full_sync { get; set; }
        public List<Item> Items { get; set; }
    }
}
