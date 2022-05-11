using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

// ReSharper disable All

namespace ServerCore
{
    public class Listener
    {
        private Func<Session> _sessionFactory;
        private Socket _socket;

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 10, int backlog = 100)
        {
            _socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(endPoint);
            _socket.Listen(backlog);

            _sessionFactory += sessionFactory;

            for (int i = 0; i < register; i++)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += OnAcceptCompleted;

                RegisterAccept(args);
            }
        }

        private void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;
            if (_socket.AcceptAsync(args) == false)
                OnAcceptCompleted(null, args);
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine($"OnAcceptCompleted failed: {args.SocketError}");
            }

            RegisterAccept(args);
        }
    }
}