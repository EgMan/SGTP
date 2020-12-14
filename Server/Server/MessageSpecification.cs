using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class MessageSpecification
    {
        /// <summary>Sent from client to server.</summary>
        public enum ClientPackets
        {
            ACTION = 1,
            REGISTER,
            STATE_ACK,
            ERROR,
            FIN,
        }

        /// <summary>Sent from server to client.</summary>
        public enum ServerPackets
        {
            STATE = 1,
            CLIENT_ACCEPT,
            CLIENT_REJECT,
            ACTION_ACK,
            ERROR,
            FIN,
        }
    }
}
