using System;
using System.Collections.Generic;
using ServerCore;

// ReSharper disable All

namespace Server
{
    public class SessionManager
    {
        public static SessionManager Instance { get; } = new SessionManager();

        private object _lock = new object();
        private int _sessionId;
        private Dictionary<int, Session> _sessions = new Dictionary<int, Session>();

        public Session Generate()
        {
            lock (_lock)
            {
                ClientSession clientSession = new ClientSession();
                clientSession.SessionId = _sessionId++;
                _sessions.Add(clientSession.SessionId, clientSession);

                Console.WriteLine($"Connected: {clientSession.SessionId}");

                return clientSession;
            }
        }
    }
}