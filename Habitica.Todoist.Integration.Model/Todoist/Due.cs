using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Habitica.Todoist.Integration.Model.Todoist
{
    public class Due
    {
        public string Date { get; set; }
        public string Timezone { get; set; }
        public string @String { get; set; }

        public string ToJavaScriptDateStr()
        {
            try { return Date + "T00:00:00.000Z"; } catch { }
            return null;
        }
    }
}
