using System;

namespace WebSocketLibNetStandard.Models
{
    public interface IWebSocketHandler
    {
        void RunServer();
        Object ReceiveObject();
    }
}
