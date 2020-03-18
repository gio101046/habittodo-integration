using System;
using System.Linq;

namespace Habitica.Todoist.Integration.Model.Todoist
{
    public class Item
    {
        public string Content { get; set; }
        public string Id { get; set; }
        public Due Due { get; set; }
        public int Is_deleted { get; set; }
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
