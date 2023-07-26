using Newtonsoft.Json;

namespace ShortLinkGen.BusinessLogic
{
    public class ShortUri
    {
        /// <summary>
        /// Токен сгенерированный для длинной ссылки
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// длинная ссылка
        /// </summary>
        [JsonProperty("source")]
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Короткая ссылка
        /// </summary>
        [JsonProperty("destination")]
        public string Destination { get; set; } = string.Empty;
    }
}
