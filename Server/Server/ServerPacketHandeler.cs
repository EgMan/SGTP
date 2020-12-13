// Code developed with help from the teachings of
// Tom Weiland's blog on unity integrated client-server architecture
// https://tomweiland.net/networking-tutorial-server-client-connection/
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class ServerPacketHandeler
    {
        public static void WelcomeRecieved(int client, Packet packet)
        {
            int clientId = packet.ReadInt();
            string username = packet.ReadString();
            Console.WriteLine($"{Server.clients[client].tcp.socket.Client.RemoteEndPoint} registered username {username}");
            if (client != clientId)
            {
                Console.WriteLine($"Client \"{username}\" has malformed id");
            }
            // todo: proceed into game
        }
    }
}
