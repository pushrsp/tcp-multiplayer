using System;
using System.Net;
using ServerCore;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            GameRoomManager.Instance.Add(1);

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 7777);

            Listener listener = new Listener();
            listener.Init(endPoint, () => SessionManager.Instance.Generate());

            Console.WriteLine("Listening....");
            while (true)
            {
            }
        }
    }
}