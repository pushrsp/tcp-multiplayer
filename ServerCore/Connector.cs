using System;
using System.Net;
using System.Net.Sockets;

// ReSharper disable All

namespace ServerCore
{
    public class Connector
    {
        private Func<Session> _sessionFactory;

        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                _sessionFactory = sessionFactory;

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += OnConnectCompleted;
                args.RemoteEndPoint = endPoint;
                args.UserToken = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                RegisterConnect(args);
            }
        }

        private void RegisterConnect(SocketAsyncEventArgs args)
        {
            Socket socket = args.UserToken as Socket;
            if (socket == null)
                return;

            if (socket.ConnectAsync(args) == false)
                OnConnectCompleted(null, args);
        }

        private void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.ConnectSocket);
                session.OnConnected(args.ConnectSocket.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine($"OnConnectCompleted failed: {args.SocketError}");
            }
        }
    }
}