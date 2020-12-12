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
        private static TcpListener listener;

        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        public static void Start(int _maxPlayers, int _port)
        {
            maxPlayers = _maxPlayers;
            port = _port;

            Console.WriteLine("Starting server...");
            Server.InitData();

            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            listener.BeginAcceptTcpClient(new AsyncCallback(OnTCPConnect), null);

            Console.WriteLine($"Server started.  Listening on port {port}");
        }

        private static void OnTCPConnect(IAsyncResult result)
        {
            TcpClient client = listener.EndAcceptTcpClient(result);
            listener.BeginAcceptTcpClient(new AsyncCallback(OnTCPConnect), null);

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

        public static void InitData()
        {
            for (int i = 1; i <= maxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }
        }
    }
}
