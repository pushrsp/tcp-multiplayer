// ReSharper disable All

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        private const int HEADER_SIZE = 2;

        public override int OnRecv(ArraySegment<byte> segment)
        {
            int processLen = 0;
            int packetCount = 0;

            while (true)
            {
                if (segment.Count < HEADER_SIZE)
                    break;

                ushort size = BitConverter.ToUInt16(segment.Array, segment.Offset);
                if (segment.Count < size)
                    break;

                OnRecvMsg(new ArraySegment<byte>(segment.Array, segment.Offset, size));
                processLen += size;
                packetCount++;
                segment = new ArraySegment<byte>(segment.Array, segment.Offset + size, segment.Count - size);
            }

            if (packetCount > 1)
                Console.WriteLine($"패킷 모아보내기: {packetCount}");

            return processLen;
        }

        public abstract void OnRecvMsg(ArraySegment<byte> segment);
    }

    public abstract class Session
    {
        private Socket _socket;

        private RecvBuffer _recvBuffer = new RecvBuffer(65535);

        private SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
        private SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

        private Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        private List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        private object _lock = new object();
        private int _disconnect;

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> segment);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += OnRecvCompleted;
            _sendArgs.Completed += OnSendCompleted;

            RegisterRecv();
        }

        public void Send(List<ArraySegment<byte>> sendList)
        {
            if (sendList.Count == 0)
                return;

            lock (_lock)
            {
                foreach (ArraySegment<byte> sendBuff in sendList)
                    _sendQueue.Enqueue(sendBuff);

                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        public void Send(ArraySegment<byte> sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        private void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnect, 1) == 1)
                return;

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            Clear();
        }

        private void Clear()
        {
            lock (_lock)
            {
                _pendingList.Clear();
                _sendQueue.Clear();
            }
        }

        private void RegisterRecv()
        {
            if (_disconnect == 1)
                return;

            _recvBuffer.Clear();
            ArraySegment<byte> write = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(write.Array, write.Offset, write.Count);

            if (_socket.ReceiveAsync(_recvArgs) == false)
                OnRecvCompleted(null, _recvArgs);
        }

        private void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                {
                    Disconnect();
                    return;
                }

                int processLen = OnRecv(_recvBuffer.ReadSegment);
                if (processLen < 0 || processLen > _recvBuffer.AllocSize)
                {
                    Disconnect();
                    return;
                }

                if (_recvBuffer.OnRead(processLen) == false)
                {
                    Disconnect();
                    return;
                }

                RegisterRecv();
            }
            else
            {
                Console.WriteLine($"OnRecvCompleted failed: {args.SocketError}");
                Disconnect();
            }
        }

        private void RegisterSend()
        {
            if (_disconnect == 1)
                return;

            while (_sendQueue.Count > 0)
                _pendingList.Add(_sendQueue.Dequeue());

            _sendArgs.BufferList = _pendingList;
            if (_socket.SendAsync(_sendArgs) == false)
                OnSendCompleted(null, _sendArgs);
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                _pendingList.Clear();
                _sendArgs.BufferList = null;

                OnSend(args.BytesTransferred);

                if (_sendQueue.Count > 0)
                    RegisterSend();
            }
            else
            {
                Console.WriteLine($"OnSendCompleted failed: {args.SocketError}");
                Disconnect();
            }
        }
    }
}