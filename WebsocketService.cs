using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSocketLibNetStandard.Model;
using System.IO;
using System.Reflection;

namespace WebSocketLibNetStandard
{
    public static class WebsocketService
    {
        public static IWebSocketHandler socketServer = null;
        private static string _uri;
        private static int _port;

        public static void CreateWebsocketHandler()
        {
            WebSocketConfigForJsonConfig websocketConfig = ConfigJson<WebSocketConfigForJsonConfig>.GetJsonFromConfigFile().Result;

            socketServer = new WebSocketTcpListener(websocketConfig);
            socketServer.RunServer(); // Start the Listener
        }
    }
}