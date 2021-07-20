using Newtonsoft.Json;

namespace Habittodo.Model.Habitica
{
    public class ChecklistItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
