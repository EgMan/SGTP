// Code developed with help from the teachings of
// Tom Weiland's blog on unity integrated client-server architecture
// https://tomweiland.net/networking-tutorial-server-client-connection/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

namespace Client
{
    public class Client : MonoBehaviour
    {
        public static Client instance;
        public static int dataBufferSize = 4096;

        public string serverIP = "127.0.0.1";
        public int serverPort = 12321;
        public int myId = 0;
        public TCP tcp;
        public UDP udp;

        private delegate void PacketHandeler(Packet packet);
        private static Dictionary<int, PacketHandeler> packetHandelers;
 
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Debug.Log("Instance already exists.  Destroying object.");
                Destroy(this);
            }
        }

        private void Start()
        {
            tcp = new TCP();
            udp = new UDP();
        }

        public void ConnectToServer()
        {

            Debug.Log($"Attempting to connect to {instance.serverIP}/{instance.serverPort}...");
            RegisterPacketHandelers();
            tcp.Connect();
        }
        private void RegisterPacketHandelers()
        {
            packetHandelers = new Dictionary<int, PacketHandeler>()
            {
                {(int)MessageSpecification.ServerPackets.CLIENT_ACCEPT, ClientPacketHandeler.ClientAccept},
                {(int)MessageSpecification.ServerPackets.CLIENT_REJECT, ClientPacketHandeler.ClientReject},
                {(int)MessageSpecification.ServerPackets.STATE, ClientPacketHandeler.State},
                {(int)MessageSpecification.ServerPackets.ACTION_ACK, ClientPacketHandeler.ActionAck},
                {(int)MessageSpecification.ServerPackets.ERROR, ClientPacketHandeler.Error},
                {(int)MessageSpecification.ServerPackets.FIN, ClientPacketHandeler.Fin},
                //{(int)ServerPackets.udpTest, ClientPacketHandeler.UDPTest},
            };
        }

        public class UDP
        {
            public UdpClient socket;
            public IPEndPoint endPoint;

            public UDP()
            {
                endPoint = new IPEndPoint(IPAddress.Parse(instance.serverIP), instance.serverPort);
            }

            public void Connect(int localPort)
            {
                socket = new UdpClient(localPort);
                socket.Connect(endPoint);
                socket.BeginReceive(OnReceiveData, null);

                using (Packet packet = new Packet())
                {
                    SendData(packet);
                }
            }

            public void SendData(Packet packet)
            {
                try
                {
                    packet.InsertInt(instance.myId);

                    if (socket != null)
                    {
                        socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error sending data to server via UDP: {e}");
                }
            }
            private void OnReceiveData(IAsyncResult result)
            {
                try
                {
                    byte[] data = socket.EndReceive(result, ref endPoint);
                    socket.BeginReceive(OnReceiveData, null);

                    if (data.Length < 4)
                    {
                        // todo this could potentially could cause problems under high traffic conditions
                        return;
                    }
                    HandleData(data);
                }
                catch (Exception e)
                {
                    Debug.Log($"Error receiving UDP data: {e}");
                }
            }
            private void HandleData(byte[] data)
            {
                using (Packet packet = new Packet(data))
                {
                    int packetLength = packet.ReadInt();
                    data = packet.ReadBytes(packetLength);
                }

                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet packet = new Packet(data))
                    {
                        int packetId = packet.ReadInt();
                        packetHandelers[packetId](packet);
                    }
                });
            }
        }

        public class TCP
        {
            public TcpClient socket;
            private NetworkStream stream;
            private byte[] receiveBuffer;
            private Packet receivedData;

            public void Connect()
            {
                socket = new TcpClient
                {
                    ReceiveBufferSize = dataBufferSize,
                    SendBufferSize = dataBufferSize,
                };

                receiveBuffer = new byte[dataBufferSize];
                socket.BeginConnect(instance.serverIP, instance.serverPort, OnConnect, socket);
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
                catch (Exception e)
                {
                    Debug.LogError($"Error sending data to server via TCP: {e}");
                }
            }

            private void OnConnect(IAsyncResult result)
            {
                socket.EndConnect(result);

                if (!socket.Connected)
                {
                    return;
                }

                Debug.Log($"Succesfully connected to {instance.serverIP}/{instance.serverPort}");

                stream = socket.GetStream();

                receivedData = new Packet();

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, OnReceiveData, null);
            }

            private void OnReceiveData(IAsyncResult result)
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
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, OnReceiveData, null);
                }
                catch (Exception e)
                {
                    Debug.Log($"Error receiving TCP data: {e}");
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
                            int id = packet.ReadInt();
                            packetHandelers[id](packet);
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