using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;


namespace Client
{
    public class ClientPacketHandeler : MonoBehaviour
    {
        public static void ClientAccept(Packet packet)
        {
            string msg = packet.ReadString();
            int id = packet.ReadInt();

            Debug.Log($"Message from server: {msg}");
            Debug.Log($"player id: {id}");
            Client.instance.myId = id;

            ClientPacketSender.Register();

            Client.instance.udp.Connect(((IPEndPoint)(Client.instance.tcp.socket.Client.LocalEndPoint)).Port);
        }

        public static void ClientReject(Packet packet)
        {
            string msg = packet.ReadString();
            Debug.Log($"Server rejected connection: {msg}");
        }
        public static void State(Packet packet)
        {
            // todo
        }
        public static void ActionAck(Packet packet)
        {
            // todo
        }
        public static void Error(Packet packet)
        {
            // todo
        }
        public static void Fin(Packet packet)
        {
            // todo
        }
        // public static void UDPTest(Packet packet)
        // {
        //     string msg = packet.ReadString();

        //     Debug.Log($"Message from server: {msg}");

        //     ClientPacketSender.UDPTestAck();
        // }
    }
}