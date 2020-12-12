// Code developed with help from the teachings of
// Tom Weiland's blog on unity integrated client-server architecture
// https://tomweiland.net/networking-tutorial-server-client-connection/
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Client
    {
        public static int dataBufferSize = 4096;

        public int id;
        public Client.TCP tcp;


        public Client(int clientId)
        {
            id = clientId;
            tcp = new TCP(id);
        }
        public class TCP
        {
            public TcpClient socket;
            private readonly int id;
            private NetworkStream stream;
            private byte[] receiveBuffer;

            public TCP(int id)
            {
                this.id = id;
            }

            public void Connect(TcpClient socket)
            {
                this.socket = socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();
                receiveBuffer = new byte[dataBufferSize];
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, OnTCPReceive, null);

                // todo welcome packet
            }

            public void OnTCPReceive(IAsyncResult result)
            {
                try
                {
                    int byteLength = stream.EndRead(result);
                    if (byteLength <= 0)
                    {
                        // todo disconnect
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);
                    // todo handle data
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, OnTCPReceive, null);
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Error receiving TCP data: {e}");
                    // todo disconnect
                }
            }
        }
    }
}
