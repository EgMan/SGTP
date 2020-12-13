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
    class Server
    {
        public static int maxPlayers { get; private set; }
        public static int port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public delegate void PacketHandeler(int fromClient, Packet packet);
        public static Dictionary<int, PacketHandeler> packetHandelers;

        private static TcpListener TCPlistener;
        private static UdpClient UDPlistener;

        public static void Start(int _maxPlayers, int _port)
        {
            maxPlayers = _maxPlayers;
            port = _port;

            Console.WriteLine("Starting server...");
            Server.InitData();

            TCPlistener = new TcpListener(IPAddress.Any, port);
            TCPlistener.Start();
            TCPlistener.BeginAcceptTcpClient(new AsyncCallback(OnTCPConnect), null);

            UDPlistener = new UdpClient(port);
            UDPlistener.BeginReceive(OnUDPReceive, null);

            Console.WriteLine($"Server started.  Listening on port {port}");
        }

        private static void OnUDPReceive(IAsyncResult result)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = UDPlistener.EndReceive(result, ref clientEndPoint);
                UDPlistener.BeginReceive(OnUDPReceive, null);
                if (data.Length < 4)
                {
                    // todo this could potentially could cause problems under high traffic conditions
                    return;
                }
                using (Packet packet = new Packet(data))
                {
                    int clientId = packet.ReadInt();
                    if (clientId == 0) return;

                    // if this connection is new, start session
                    if (clients[clientId].udp.endPoint == null)
                    {
                        clients[clientId].udp.Connect(clientEndPoint);
                        return;
                    }

                    // todo additional security might be needed to prevent ip spoofing attack
                    if (clients[clientId].udp.endPoint.ToString() == clientEndPoint.ToString())
                    {
                        clients[clientId].udp.HandleData(packet);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error receiving UDP data: {e}");
            }
        }

        public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
        {
            try
            {
                if (clientEndPoint != null)
                {
                    UDPlistener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error sending UDP data: {e}");
            }
        }

        private static void OnTCPConnect(IAsyncResult result)
        {
            TcpClient client = TCPlistener.EndAcceptTcpClient(result);
            TCPlistener.BeginAcceptTcpClient(new AsyncCallback(OnTCPConnect), null);

            Console.WriteLine($"{client.Client.RemoteEndPoint} is attempting to connect.");
            for (int i = 1; i <= maxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(client);
                    Console.WriteLine($"Connection established to {client.Client.RemoteEndPoint}.");
                    return;
                }
            }
            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server at capacity");
        }

        private static void InitData()
        {
            for (int i = 1; i <= maxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }
            packetHandelers = new Dictionary<int, PacketHandeler>()
            {
                {(int)ClientPackets.welcomeReceived, ServerPacketHandeler.WelcomeRecieved},
                {(int)ClientPackets.udpTestAck, ServerPacketHandeler.UDPTestAck},
            };
        }
    }
}
