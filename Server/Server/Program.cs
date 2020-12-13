// Code developed with help from the teachings of
// Tom Weiland's blog on unity integrated client-server architecture
// https://tomweiland.net/networking-tutorial-server-client-connection/
using System;
using System.Threading;

namespace Server
{
    class Program
    {
        private static bool running = false;
        static void Main(string[] args)
        {
            Console.Title = "Game Server";

            running = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Thread inputThread = new Thread(new ThreadStart(UserInputThread));
            inputThread.Start();

            Server.Start(8, Constants.PORT_NUMBER);
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main thread started.  Running at {Constants.TICKS_PER_SEC} ticks per second.");
            while (running)
            {
                ThreadManager.UpdateMain();
                Thread.Sleep(Constants.MS_PER_TICK);
            }
            Console.WriteLine($"Server closed.");
        }

        private static void UserInputThread()
        {
            while (running)
            {
                string[] cmd = Console.ReadLine().Split(" ");
                switch (cmd[0])
                {
                    case "quit":
                        Console.WriteLine("Closing server...");
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Unrecognized command");
                        break;
                }
            }
        }
    }
}
