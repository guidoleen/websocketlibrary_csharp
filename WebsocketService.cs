using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSocketLibNetStandard.Models;

namespace WebSocketLibNetStandard
{
    public static class WebsocketService
    {
        public static IWebSocketHandler socketServer = null;
        private static string _uri = "127.0.0.1";
        private static int _port = 3000;

        public static void CreateWebsocketHandler()
        {
            socketServer = new WebSocketTcpListener(_uri, _port);
            socketServer.RunServer(); // Start the Listener
        }
    }
}