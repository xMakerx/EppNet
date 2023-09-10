///////////////////////////////////////////////////////
/// Filename: BytePayload.cs
/// Date: September 10, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using Microsoft.IO;

using System;
using System.IO;
using System.Text;

namespace EppNet.Data
{

    public class BytePayload : IDisposable
    {

        #region Static members
        public static readonly RecyclableMemoryStreamManager RecyclableStreamMgr = new RecyclableMemoryStreamManager();
        #endregion

        public Encoding Encoder;

        protected internal MemoryStream _stream;
        protected internal BinaryWriter _writer;
        protected internal BinaryReader _reader;

        public BytePayload()
        {
            this.Encoder = Encoding.UTF8;
            this._stream = null;
            this._writer = null;
            this._reader = null;
        }

        public BytePayload(byte[] dataIn) : this()
        {
            _stream = RecyclableStreamMgr.GetStream(Guid.NewGuid(), "", dataIn.Length, true);
            _stream.Write(dataIn);
            _stream.Position = 0;
            _stream.Close();

            _reader = new BinaryReader(_stream);
        }

        public virtual void Dispose()
        {
            _reader?.Dispose();
            _writer?.Dispose();
            _stream?.Dispose();
        }

        private void _EnsureReadyToWrite()
        {
            if (_stream == null)
                _stream = RecyclableStreamMgr.GetStream();

            if (_writer == null)
                _writer = new BinaryWriter(_stream, Encoder);
        }

        public void WriteString(string input)
        {
            if (input.Length == 0 || input.Length > ushort.MaxValue)
                throw new ArgumentOutOfRangeException($"Datagram#WriteString(string) must be between 1 and {ushort.MaxValue}.");

            _EnsureReadyToWrite();
            _writer.Write(input);
        }

        public string ReadString() => _reader.ReadString();

        public void WriteBool(bool input)
        {
            _EnsureReadyToWrite();
            _writer.Write(input);
        }

        public bool ReadBool() => _reader.ReadBoolean();

        public void WriteUInt8(byte input)
        {
            _EnsureReadyToWrite();
            _writer.Write(input);
        }

        public byte ReadUInt8() => _reader.ReadByte();

        public void WriteByte(byte input) => WriteUInt8(input);
        public byte ReadByte() => ReadUInt8();

        public void WriteInt8(sbyte input)
        {
            _EnsureReadyToWrite();
            _writer.Write(input);
        }

        public sbyte ReadInt8() => _reader.ReadSByte();

        public void WriteSByte(sbyte input) => WriteInt8(input);
        public sbyte ReadSByte() => ReadInt8();

        public void WriteInt16(short input)
        {
            _EnsureReadyToWrite();
            _writer.Write(input);
        }

        public short ReadInt16() => _reader.ReadInt16();

        public void WriteShort(short input) => WriteInt16(input);
        public short ReadShort() => ReadInt16();

        public void WriteUInt16(ushort input)
        {
            _EnsureReadyToWrite();
            _writer.Write(input);
        }

        public ushort ReadUInt16() => _reader.ReadUInt16();

        public void WriteUShort(ushort input) => WriteUInt16(input);
        public ushort ReadUShort() => ReadUInt16();

        public void WriteUInt32(uint input)
        {
            _EnsureReadyToWrite();
            _writer.Write(input);
        }

        public uint ReadUInt32() => _reader.ReadUInt32();

        public void WriteUInt(uint input) => WriteUInt32(input);
        public uint ReadUInt() => ReadUInt32();

        public void WriteInt32(int input)
        {
            _EnsureReadyToWrite();
            _writer.Write(input);
        }

        public int ReadInt32() => _reader.ReadInt32();

        public void WriteInt(int input) => WriteInt32(input);
        public int ReadInt() => ReadInt32();

        public void WriteUInt64(ulong input)
        {
            _EnsureReadyToWrite();
            _writer.Write(input);
        }

        public ulong ReadUInt64() => _reader.ReadUInt64();

        public void WriteULong(ulong input) => WriteUInt64(input);
        public ulong ReadULong() => ReadUInt64();

        public void WriteInt64(long input)
        {
            _EnsureReadyToWrite();
            _writer.Write(input);
        }

        public long ReadInt64() => _reader.ReadInt64();

        public void WriteLong(long input) => WriteInt64(input);
        public long ReadLong() => ReadInt64();

        public void WriteFloat(float input)
        {
            _EnsureReadyToWrite();
            _writer.Write(input);
        }

        public void WriteSingle(float input) => WriteFloat(input);

        public float ReadFloat() => _reader.ReadSingle();
        public float ReadSingle() => ReadFloat();
    }

}