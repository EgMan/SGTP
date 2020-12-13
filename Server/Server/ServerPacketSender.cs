// Code developed with help from the teachings of
// Tom Weiland's blog on unity integrated client-server architecture
// https://tomweiland.net/networking-tutorial-server-client-connection/
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class ServerPacketSender
    {
        private static void TCPSendData(int client, Packet packet)
        {
            packet.WriteLength();
            Server.clients[client].tcp.SendData(packet);
        }

        private static void TCPBroadcast(Packet packet)
        {
            TCPMulticast(Server.clients.Values, packet);
        }
        private static void TCPMulticast(IEnumerable<Client> clients, Packet packet)
        {
            packet.WriteLength();
            foreach (Client client in clients)
            {
                client.tcp.SendData(packet);
            }
        }

        public static void Welcome(int client, string msg)
        {
            using (Packet packet = new Packet((int)ServerPackets.welcome))
            {
                packet.Write(msg);
                packet.Write(client);

                TCPSendData(client, packet);
            }
        }
    }
}
