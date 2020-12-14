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
        public Client.UDP udp;


        public Client(int clientId)
        {
            id = clientId;
            tcp = new TCP(id);
            udp = new UDP(id);
        }
        public class UDP
        {
            public IPEndPoint endPoint;
            private int id;

            public UDP(int id)
            {
                this.id = id;
            }

            public void Connect(IPEndPoint endPoint)
            {
                this.endPoint = endPoint;

                //ServerPacketSender.UDPTest(id);
            }

            public void SendData(Packet packet)
            {
                Server.SendUDPData(endPoint, packet);
            }

            public void HandleData(Packet packet)
            {
                int packetLength = packet.ReadInt();
                byte[] packetBytes = packet.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int messageType = packet.ReadInt();
                        Server.packetHandelers[messageType](id, packet);
                    }
                });
            }
        }
        public class TCP
        {
            public TcpClient socket;
            private readonly int id;
            private NetworkStream stream;
            private byte[] receiveBuffer;
            private Packet receivedData;

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

                receivedData = new Packet();

                ServerPacketSender.ClientAccept(id, "Connection successful");
            }

            public void SendData(Packet packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Error sending data to client {id} at {socket.Client.RemoteEndPoint}: {e}");
                }
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

                    receivedData.Reset(HandleData(data));

                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, OnTCPReceive, null);
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Error receiving TCP data: {e}");
                    // todo disconnect
                }
            }

            private bool HandleData(byte[] data)
            {
                int packetLength = 0;
                receivedData.SetBytes(data);
                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
                while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
                {
                    byte[] packetBytes = receivedData.ReadBytes(packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int messageType = packet.ReadInt();
                            Server.packetHandelers[messageType](id, packet);
                        }
                    });
                    packetLength = 0;

                    if (receivedData.UnreadLength() >= 4)
                    {
                        packetLength = receivedData.ReadInt();
                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (packetLength <= 1)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
