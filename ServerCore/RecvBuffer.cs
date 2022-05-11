using System;

namespace ServerCore
{
    public class RecvBuffer
    {
        private ArraySegment<byte> _buffer;
        private int _readPos;
        private int _writePos;

        public RecvBuffer(int size)
        {
            _buffer = new ArraySegment<byte>(new byte[size], 0, size);
        }

        public int AllocSize
        {
            get { return _writePos - _readPos; }
        }

        public int FreeSize
        {
            get { return _buffer.Count - _writePos; }
        }

        public ArraySegment<byte> ReadSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, AllocSize); }
        }

        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        public void Clear()
        {
            int allocSize = AllocSize;
            if (allocSize == 0)
            {
                _readPos = _writePos = 0;
            }
            else
            {
                Array.Copy(_buffer.Array, _readPos, _buffer.Array, 0, allocSize);
                _readPos = 0;
                _writePos = allocSize;
            }
        }

        public bool OnRead(int numBytes)
        {
            if (numBytes > AllocSize)
                return false;
            _readPos += numBytes;
            return true;
        }

        public bool OnWrite(int numBytes)
        {
            if (numBytes > FreeSize)
                return false;
            _writePos += numBytes;
            return true;
        }
    }
}