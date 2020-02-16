using System;
using Newtonsoft.Json;

namespace WebSocketLibNetStandard.Model
{
    public class WebSocketConfigForJsonConfig
    {
        [JsonProperty("WebsocketConfig")]
        public WebSocketConfigForJson WebSocketConfigForJson { get; set; }
    }
}
