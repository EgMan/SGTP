using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Client
{
    public class ClientPacketSender : MonoBehaviour
    {
        public static void TCPSendData(Packet packet)
        {
            packet.WriteLength();
            Client.instance.tcp.SendData(packet);
        }
        public static void SendUDPData(Packet packet)
        {
            packet.WriteLength();
            Client.instance.udp.SendData(packet);
        }

        public static void Register()
        {
            using (Packet packet = new Packet((int)MessageSpecification.ClientPackets.REGISTER))
            {
                packet.Write(Client.instance.myId);
                packet.Write(UIManager.instance.userNameField.text);

                TCPSendData(packet);
            }
        }
        // public static void UDPTestAck()
        // {
        //     using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
        //     {
        //         packet.Write("This is a test right back at you!");
        //         TCPSendData(packet);
        //     }
        // }
        
        
        public static void Action()
        {
            // todo
        }
        public static void StateAck()
        {
            // todo
        }
        public static void Error()
        {
            // todo
        }
        public static void Fin()
        {
            // todo
        }
    }
}