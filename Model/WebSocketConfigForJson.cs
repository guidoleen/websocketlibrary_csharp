using Newtonsoft.Json;
using System.Collections.Generic;

namespace WebSocketLibNetStandard.Model
{
    // [JsonObject(MemberSerialization.OptIn)]
    public class WebSocketConfigForJson
    {
        [JsonProperty("uri")]
        public string TcpAddress { get; set; }
        [JsonProperty("port")]
        public int Port { get; set; }
        [JsonProperty("socketconkey")]
        public string SocketConnectionKey { get; set; }
        [JsonProperty("bytebuffer")]
        public int ByteBuffer { get; set; }
        [JsonProperty("certificate")]
        public string CertificateFileName { get; set; }

        [JsonProperty("originacceptlist")]
        public List<string> AcceptUriList { get; set; }

        [JsonProperty("originurischeme")]
        public List<string> AcceptUriScheme { get; set; }

        [JsonProperty("certificatekey")]
        public string CertificatePwd { get; set; }
    }
}