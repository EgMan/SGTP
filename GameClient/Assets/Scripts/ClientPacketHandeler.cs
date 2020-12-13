using System.Collections;
using System.Collections.Generic;
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
        }
    }
}