using Newtonsoft.Json;

namespace Habittodo.Model.Habitica.Responses
{
    public class HabiticaReponse<T>
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("data")]
        public T Data { get; set; }
        [JsonProperty("notifications")]
        public object Notifications { get; set; }
    }
}
