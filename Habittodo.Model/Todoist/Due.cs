using Newtonsoft.Json;

namespace Habittodo.Model.Todoist
{
    public class Due
    {
        [JsonProperty("date")]
        public string Date { get; set; }
        [JsonProperty("timezone")]
        public string Timezone { get; set; }
        [JsonProperty("string")]
        public string @String { get; set; }

        public string ToJavaScriptDateStr()
        {
            try { return Date + "T00:00:00.000Z"; } catch { }
            return null;
        }
    }
}
