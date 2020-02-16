using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using WebSocketLibNetStandard.Model;
using System.Net.Security;
using System.Configuration;

namespace WebSocketLibNetStandard.Model
{
    public class WebSocketTcpListener : IWebSocketHandler
    {
        private static TcpListener _websocketListener;
        private static string _tcpaddress;
        private static int _port;
        private static string _socketconnectionkey; // Special code based on the RFC compliance
        private static List<string> _clientoriginaccept;
        private static List<string> _clientoriginscheme;
        private static int _byte_buffer;
        private static string _certificate_filename;
        private static string _certificate_pwd;

        public WebSocketTcpListener(WebSocketConfigForJsonConfig websocketConfig)
        {
            _tcpaddress = websocketConfig.WebSocketConfigForJson.TcpAddress;
            _port = websocketConfig.WebSocketConfigForJson.Port;
            _socketconnectionkey = websocketConfig.WebSocketConfigForJson.SocketConnectionKey;
            _clientoriginaccept = websocketConfig.WebSocketConfigForJson.AcceptUriList;
            _clientoriginscheme = websocketConfig.WebSocketConfigForJson.AcceptUriScheme;

            _byte_buffer = websocketConfig.WebSocketConfigForJson.ByteBuffer;
            _certificate_filename = websocketConfig.WebSocketConfigForJson.CertificateFileName;
            _certificate_pwd = TextEncoderDecoder.Decode(websocketConfig.WebSocketConfigForJson.CertificatePwd);
        }

        public WebSocketTcpListener(string tcpaddress, int port)
        {
            _tcpaddress = tcpaddress;
            _port = port;
        }

        private static void SetTcpListener()
        {
            _websocketListener = new TcpListener(IPAddress.Parse(_tcpaddress), _port);
            _websocketListener.Start();

                Console.WriteLine($"A server has started");
        }
        
        private static void StopTcpListener()
        {
            _websocketListener.Stop();
        }

        public object ReceiveObject()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 1) First run the TcpListener 2) Run the TcpClient server
        /// </summary>

        // Running the listener
        public void RunServer()
        {
            SetTcpListener();
            HandleClientFromTcpListener();
        }

        private void HandleClientFromTcpListener()
        {
            Task<TcpClient> client = null; // default(TcpClient);
            int counter = 0;

            while (true)
            {
                counter++;

                client = _websocketListener.AcceptTcpClientAsync(); // AcceptTcpClient();
                Console.WriteLine($"A client is accepted: {counter.ToString()}");

                HandleClient(client.Result);
            }
        }

        async private void HandleClient(TcpClient client)
        {
            int requestCount = 0;
            bool isAccepableClient = true;

            NetworkStream stream = default(NetworkStream);
            await Task.Run(() =>
            {
                while (true && isAccepableClient)
                {
                    if (client.Client.Connected)
                    {
                        try
                        {
                            while (client.Available <= 3) ;

                            byte[] buffer = new byte[client.Available];

                            stream = client.GetStream();
                            stream.Read(buffer, 0, client.Available);

                                CreateHandshakeAndReadWrite(buffer, stream);

                            requestCount++;
                        }
                        catch (Exception ee)
                        {
                            Console.WriteLine(ee.ToString());
                            client.Close();
                            stream.Close();
                        }
                    }
                }
            });
        }

        // TODO: SSL implementation: Stil in progress for creating a secured stream based on Https > Check the right port 443 for this
        private void GetSslStreamFromClient(TcpClient client, byte[] buffer)
        {
            // Check if SslCertificate is value or not
            SslStream sslStream = new SslStream(client.GetStream());
            sslStream.AuthenticateAsServer(WebSocketCertificate.CreateServerCertificate(_certificate_filename, _certificate_pwd));

            // Read bytes from stream
            sslStream.Read(buffer, 0, client.Available);
        }

        private void CreateHandshakeAndReadWrite(byte[] buffer, NetworkStream stream)
        {
            bool isAcceptableClient = false; // Checking for client origin
            var strInputFromClient = Encoding.UTF8.GetString(buffer);

            // HANDSHAKE >> Check if handshake necessary
            // 1 .Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
            // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
            // 3. Compute SHA-1 and Base64 hash of the new value
            // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
            if (Regex.IsMatch(strInputFromClient, "^GET", RegexOptions.IgnoreCase))
            {
                // First check from Origin Header information if the client is authorized based on its original uri
                string strClientOrigin = Regex.Match(strInputFromClient, "Origin: (.*)").Groups[1].Value.Trim();
                isAcceptableClient = IsAcceptableClient(strClientOrigin);
                if (!isAcceptableClient) return; // Stop the further handshake process

                string strSecretClientKey = Regex.Match(strInputFromClient, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();

                byte[] writeBytes = ResponseHeaderToBytes(strSecretClientKey + _socketconnectionkey);

                // Write resonseheader back to the client as handshake
                stream.Write(writeBytes, 0, writeBytes.Length);
            }
            else
            {
                // Write to Client
                byte[] writeBytes = this.WriteBytesToBrowser($"TestBytes<b>New Test Bytes</b>");
                if (writeBytes != null)
                    stream.Write(writeBytes, 0, writeBytes.Length);

                // Text from client to server
                Console.Write(ReadBytesFromClient(buffer));
            }
        }

        private bool IsAcceptableClient(string strclientorigin)
        {
            string regexOnConfig = "";
            bool isAcceptableClient = false;

            foreach(var configuri in _clientoriginaccept)
            {
                foreach(var configscheme in _clientoriginscheme)
                {
                    regexOnConfig = $"^{configscheme}://{configuri}";
                    isAcceptableClient = Regex.Match(strclientorigin, regexOnConfig, RegexOptions.IgnoreCase).Success;
                    if (isAcceptableClient)
                    {
                        return true;
                    }
                }
            }
            return isAcceptableClient;
        }

        /// <summary>
        /// WriteBytesToBrowser As a frame > Websocket working with frames
        /// </summary>
        /// <param name="message"></param>
        /// <param name="opcode"></param>
        /// <returns></returns>
        private byte[] WriteBytesToBrowser(string message, EOpcodeType opcode = EOpcodeType.Text)
        {
            byte[] response;
            byte[] bytesRaw = Encoding.Default.GetBytes(message);
            byte[] frame = new byte[10];

            int indexStartRawData = -1;
            int length = bytesRaw.Length;

            frame[0] = (byte)(128 + (int)opcode);
            if (length <= 125)
            {
                frame[1] = (byte)length;
                indexStartRawData = 2;
            }
            else if (length >= 126 && length <= 65535)
            {
                frame[1] = (byte)126;
                frame[2] = (byte)((length >> 8) & 255);
                frame[3] = (byte)(length & 255);
                indexStartRawData = 4;
            }
            else
            {
                frame[1] = (byte)127;
                frame[2] = (byte)((length >> 56) & 255);
                frame[3] = (byte)((length >> 48) & 255);
                frame[4] = (byte)((length >> 40) & 255);
                frame[5] = (byte)((length >> 32) & 255);
                frame[6] = (byte)((length >> 24) & 255);
                frame[7] = (byte)((length >> 16) & 255);
                frame[8] = (byte)((length >> 8) & 255);
                frame[9] = (byte)(length & 255);

                indexStartRawData = 10;
            }

            response = new byte[indexStartRawData + length];

            int i, reponseIdx = 0;

            //Add the frame bytes to the reponse
            for (i = 0; i < indexStartRawData; i++)
            {
                response[reponseIdx] = frame[i];
                reponseIdx++;
            }

            //Add the data bytes to the response
            for (i = 0; i < length; i++)
            {
                response[reponseIdx] = bytesRaw[i];
                reponseIdx++;
            }

            return response;
        }

        //function to create  frames to send to client 
        /// <summary>
        /// Enum for opcode types
        /// </summary>
        public enum EOpcodeType
        {
            /* Denotes a continuation code */
            Fragment = 0,

            /* Denotes a text code */
            Text = 1,

            /* Denotes a binary code */
            Binary = 2,

            /* Denotes a closed connection */
            ClosedConnection = 8,

            /* Denotes a ping*/
            Ping = 9,

            /* Denotes a pong */
            Pong = 10
        }

        private byte[] ResponseHeaderToBytes(string secretKeyFromClient)
        {
            // String key >> SHA1 >> Hash >> Base64 for acceptance in browser
            string secretKeyBase64 = Convert.ToBase64String(
                                    System.Security.Cryptography.SHA1.Create().ComputeHash(
                                    Encoding.UTF8.GetBytes(secretKeyFromClient
                                    )));

            string newline = "\r\n";
            byte[] response = Encoding.UTF8.GetBytes(
                    "HTTP/1.1 101 Swiching Protocols" + newline +
                    "Connection: Upgrade" + newline +
                    "Upgrade: websocket" + newline +
                    "Sec-Websocket-Accept: " + secretKeyBase64 + newline
                    + newline
                );

            return response;
        }

        // This method reads the bytes from the stream and convert them into text with a max length of 65535 chars
        private object ReadBytesFromClient(byte[] bytes)
        {
            bool mask = (bytes[1] & 0b10000000) != 0; // must be true, "All messages from the client to the server have this bit set"
            int msglen = bytes[1] - 128; // & 0111 1111
            int offset = 2;

            if (msglen == 126) // When messageLength is larger than one byte to store but posible for two bytes eg. smaller than 65535
            {
                msglen = BitConverter.ToUInt16(new byte[] { bytes[3], bytes[2] }, 0);
                offset = 4;
            }
            else if (msglen == 127) // When messageLength is larger than two bytes to store than take 6 >> When message is larger than 65535
            {
                return null;
            }

            if (msglen == 0)
                return null;

            else if (mask) // Execute the message to the Server with the available bytes
            {
                byte[] decoded = new byte[msglen];
                byte[] masks = new byte[4] { bytes[offset], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3] };
                offset += 4;

                for (int i = 0; i < msglen; ++i)
                    decoded[i] = (byte)(bytes[offset + i] ^ masks[i % 4]);

                return Encoding.UTF8.GetString(decoded); // Return the decoded string eg. Json
            }
            else
                Console.WriteLine("Can't handle the message from Client.");

           return null;
        }

        private byte[] SendObjectToBrowser(object obj)
        {
            return System.Text.Encoding.UTF8.GetBytes(obj.ToString());
        }
    }
}

///// http://csharp.net-informations.com/communications/csharp-multi-threaded-server-socket.htm
///// https://docs.microsoft.com/en-us/dotnet/api/system.net.security.sslstream?view=netframework-4.8
///// https://docs.microsoft.com/en-us/dotnet/api/system.net.security.sslstream?redirectedfrom=MSDN&view=netframework-4.8
///// https://www.ssls.com/knowledgebase/how-can-i-find-the-private-key-for-my-ssl-certificate/
///// https://stackoverflow.com/questions/10200910/creating-a-hello-world-websocket-example
///// https://tools.ietf.org/html/rfc6455#section-5.2