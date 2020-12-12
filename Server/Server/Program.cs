// Code developed with help from the teachings of
// Tom Weiland's blog on unity integrated client-server architecture
// https://tomweiland.net/networking-tutorial-server-client-connection/
using System;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Game Server";
            Server.Start(8, 12321);
            Console.ReadKey();
        }
    }
}
