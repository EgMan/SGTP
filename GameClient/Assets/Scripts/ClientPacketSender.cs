﻿using System.Collections;
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
        public static void WelcomeACK()
        {
            using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
            {
                packet.Write(Client.instance.myId);
                packet.Write(UIManager.instance.userNameField.text);

                TCPSendData(packet);
            }
        }
    }
}