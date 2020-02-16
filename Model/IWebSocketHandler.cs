using System;

namespace WebSocketLibNetStandard.Model
{
    public interface IWebSocketHandler
    {
        void RunServer();
        Object ReceiveObject();
    }
}
