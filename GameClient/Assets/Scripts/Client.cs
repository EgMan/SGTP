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

        public string ip = "127.0.0.1";
        public int port = 12321;
        public int myId = 0;
        public TCP tcp;

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
        }

        public void ConnectToServer()
        {

            Debug.Log($"Attempting to connect to {instance.ip}/{instance.port}...");
            RegisterPacketHandelers();
            tcp.Connect();
        }
        private void RegisterPacketHandelers()
        {
            packetHandelers = new Dictionary<int, PacketHandeler>()
            {
                {(int)ServerPackets.welcome, ClientPacketHandeler.Welcome}
            };
        }

        public class TCP
        {
            TcpClient socket;
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
                socket.BeginConnect(instance.ip, instance.port, OnTCPConnect, socket);
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
                    Debug.LogError($"Error sending data to server: {e}");
                }
            }

            private void OnTCPConnect(IAsyncResult result)
            {
                socket.EndConnect(result);

                if (!socket.Connected)
                {
                    return;
                }

                Debug.Log($"Succesfully connected to {instance.ip}/{instance.port}");

                stream = socket.GetStream();

                receivedData = new Packet();

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, OnTCPReceive, null);
            }

            private void OnTCPReceive(IAsyncResult result)
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