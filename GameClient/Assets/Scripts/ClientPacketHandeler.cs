using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;


namespace Client
{
    public class ClientPacketHandeler : MonoBehaviour
    {
        public static void Welcome(Packet packet)
        {
            string msg = packet.ReadString();
            int id = packet.ReadInt();

            Debug.Log($"Message from server: {msg}");
            Client.instance.myId = id;

            ClientPacketSender.WelcomeACK();

            Client.instance.udp.Connect(((IPEndPoint)(Client.instance.tcp.socket.Client.LocalEndPoint)).Port);
        }
        public static void UDPTest(Packet packet)
        {
            string msg = packet.ReadString();

            Debug.Log($"Message from server: {msg}");

            ClientPacketSender.UDPTestAck();
        }
    }
}