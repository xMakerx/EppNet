///////////////////////////////////////////////////////
/// Filename: BytePayload.cs
/// Date: September 10, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
/// <summary>
/// Performant I/O object for reading & writing C# primitives.
/// Leverages the functionality of Microsoft's <see cref="RecyclableMemoryStream"/>
/// and stack allocated spans to quickly read and write from a binary buffer.
/// While some functions such as #WriteTimestamp(<see cref="Timestamp"/>) exist, these objects
/// aren't serialized, they're broken down into primitives necessary
/// to reconstruct the object.
/// 
/// Binary data is exclusively written and read in little endian to
/// facilitate networking with computers with different CPUs.
/// </summary>


using EppNet.Utilities;

using Microsoft.IO;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace EppNet.Data
{

    public class BytePayload : IDisposable
    {

        #region Static members
        public static RecyclableMemoryStreamManager RecyclableStreamMgr { get; }

        private static readonly Dictionary<Type, IResolver> _resolvers = new Dictionary<Type, IResolver>();

        static BytePayload()
        {

            RecyclableMemoryStreamManager.Options options = new RecyclableMemoryStreamManager.Options();
            options.ZeroOutBuffer = false;
            options.BlockSize = 512; // 0.5 KB blocks
            options.LargeBufferMultiple = 2048; // Large buffers are multiples of 2 KB
            options.MaximumBufferSize = 12 * 1024 * 1024; // 12MB is too large to be repooled.
            options.UseExponentialLargeBuffer = false; // Linear growth strategy
            options.MaximumSmallPoolFreeBytes = 64 * 1024 * 1024; // Keep 64 MB worth of small pools around
            options.MaximumLargePoolFreeBytes = 128 * 1024 * 1024; // Keep 128MB worth of large pools around

            // Kills performance when enabled. Helpful in debugging situations
            options.GenerateCallStacks = false;

            RecyclableStreamMgr = new(options);

            _resolvers.Add(typeof(bool), BoolResolver.Instance);
            _resolvers.Add(typeof(byte), ByteResolver.Instance);
            _resolvers.Add(typeof(sbyte), SByteResolver.Instance);
            _resolvers.Add(typeof(ushort), UShortResolver.Instance);
            _resolvers.Add(typeof(short), ShortResolver.Instance);
            _resolvers.Add(typeof(uint), UInt32Resolver.Instance);
            _resolvers.Add(typeof(int), Int32Resolver.Instance);
            _resolvers.Add(typeof(ulong), ULongResolver.Instance);
            _resolvers.Add(typeof(long), LongResolver.Instance);
            _resolvers.Add(typeof(float), FloatResolver.Instance);
            _resolvers.Add(typeof(Color), ColorResolver.Instance);
            _resolvers.Add(typeof(Str8), String8Resolver.Instance);
            _resolvers.Add(typeof(Str16), String16Resolver.Instance);
            _resolvers.Add(typeof(Quaternion), QuaternionResolver.Instance);
            _resolvers.Add(typeof(Vector2), Vector2Resolver.Instance);
            _resolvers.Add(typeof(Vector3), Vector3Resolver.Instance);
            _resolvers.Add(typeof(Vector4), Vector4Resolver.Instance);
            _resolvers.Add(typeof(Guid), GuidResolver.Instance);
        }

        public static void AddResolver(Type type, IResolver resolver)
            => _resolvers.Add(type, resolver);

        public static IResolver GetResolver(Type type)
        {
            _resolvers.TryGetValue(type, out IResolver result);
            return result;
        }

        public static Resolver<T> GetResolver<T>()
        {
            Type type = typeof(T);

            if (type.IsArray)
                type = type.GetElementType();
            
            else if (type.IsEnum)
                type = type.GetEnumUnderlyingType();

            else if (type.IsGenericType)
            {

                // Supported collection types
                Type genType = type.GetGenericTypeDefinition();

                if (genType == typeof(List<>) || genType == typeof(HashSet<>) || genType == typeof(SortedSet<>) || genType == typeof(LinkedList<>))
                    type = type.GetGenericArguments()[0];
            }

            _resolvers.TryGetValue(type, out IResolver result);
            return (Resolver<T>)result;
        }

        /// <summary>
        /// The decimal precision of floating point numbers.
        /// </summary>
        public static int FloatPrecision = 4;

        public static double PrecisionDecimalPlaces
        {
            get
            {
                if (_precisionDecimalPlaces == 0)
                    _precisionDecimalPlaces = FastMath.GetTenPow(FloatPrecision);

                return _precisionDecimalPlaces;
            }
        }

        private static double _precisionDecimalPlaces = 0d;

        public static double PrecisionReturnDecimalPlaces
        {
            get
            {
                if (_precisionReturnDecimalPlaces == 0)
                    _precisionReturnDecimalPlaces = 1 / PrecisionDecimalPlaces;

                return _precisionReturnDecimalPlaces;
            }
        }

        private static double _precisionReturnDecimalPlaces = 0d;

        public static RecyclableMemoryStream ObtainStream() => RecyclableStreamMgr.GetStream();

        public static RecyclableMemoryStream ObtainStream(byte[] buffer) => RecyclableStreamMgr.GetStream(buffer);

        /// <summary>
        /// Converts a local float to a remote float (i.e. a float with the precision specified by <see cref="FloatPrecision"/>)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>

        public static float FloatToNetFloat(float input)
        {
            int a = (int)(input.Round(FloatPrecision) * PrecisionDecimalPlaces);
            return a * (float) PrecisionReturnDecimalPlaces;
        }

        #endregion

        /// <summary>
        /// The encoding this payload uses when handling strings.
        /// <br/>Default is <see cref="Encoding.UTF8"/><br/><br/>
        /// Do not change the encoder after you've already begun I/O operations
        /// on this payload as that's unexpected behavior.
        /// </summary>

        public Encoding Encoder = Encoding.UTF8;

        /// <summary>
        /// A byte array representing the data held by the internal <see cref="RecyclableMemoryStream"/>.<br/>
        /// This is populated by <see cref="Pack"/>.
        /// </summary>
        public byte[] PackedData { internal set; get; }

        /// <summary>
        /// The length of this payload in bytes
        /// </summary>

        public long Length { get => Stream != null ? Stream.Length : 0; }

        public RecyclableMemoryStream Stream { protected set; get; }

        /// <summary>
        /// Initializes an empty new BytePayload. A memory stream is not fetched from the pool
        /// until a write method is called.
        /// </summary>

        public BytePayload()
        {
            this.PackedData = null;
            this.Stream = null;
        }

        /// <summary>
        /// Initializes a new BytePayload with the provided byte array allocated into an obtained
        /// <see cref="RecyclableMemoryStream"/>
        /// </summary>
        /// <param name="dataIn"></param>

        public BytePayload(byte[] dataIn) : this()
        {
            this.PackedData = dataIn;
            this.Stream = RecyclableStreamMgr.GetStream(dataIn);
            this.Stream.Seek(0, System.IO.SeekOrigin.Begin);
        }

        public void ReadFrom([NotNull] byte[] dataIn)
        {
            Stream = ObtainStream(dataIn);
            Stream.Seek(0, System.IO.SeekOrigin.Begin);
        }

        public bool Advance(int byteLength)
        {
            if (Stream == null)
                return false;

            Stream.Advance(byteLength);
            return true;
        }

        public virtual bool TryWriteDictionary<TKey, TValue>(in IDictionary<TKey, TValue> dict)
        {
            Resolver<TKey> keyResolver = GetResolver<TKey>();
            Resolver<TValue> valueResolver = GetResolver<TValue>();

            if (keyResolver == null || valueResolver == null)
            {
                Serilog.Log.Warning($"BytePayload#TryWriteDictionary(): Cannot resolve IDictionary<{typeof(TKey).Name}, {typeof(TValue).Name}>! Do you have resolvers for both types?");
                return false;
            }

            if (dict == null || dict.Count == 0)
            {
                byte header = dict == null
                    ? IResolver.NullArrayHeader
                    : IResolver.EmptyArrayHeader;
                Stream.WriteByte(header);
                return true;
            }

            IResolver._Internal_WriteHeaderAndLength(this, dict.Count);
            bool shouldContinue = true;

            foreach (KeyValuePair<TKey, TValue> kvp in dict)
            {
                // Write key value pairs next to each other.
                shouldContinue = keyResolver.Write(this, kvp.Key);

                if (!shouldContinue)
                    break;

                shouldContinue = valueResolver.Write(this, kvp.Value);
            }

            return shouldContinue;
        }

        public virtual bool TryWriteDictionary(in IDictionary dict, in Type keyType, in Type valueType)
        {
            IResolver keyResolver = GetResolver(keyType);
            IResolver valueResolver = GetResolver(valueType);

            if (keyResolver == null || valueResolver == null)
            {
                Serilog.Log.Warning($"BytePayload#TryWriteDictionary(): Cannot resolve IDictionary<{keyType.Name}, {valueType.Name}>! Do you have resolvers for both types?");
                return false;
            }

            if (dict == null || dict.Count == 0)
            {
                byte header = dict == null
                    ? IResolver.NullArrayHeader
                    : IResolver.EmptyArrayHeader;
                Stream.WriteByte(header);
                return true;
            }

            IResolver._Internal_WriteHeaderAndLength(this, dict.Count);
            bool shouldContinue = true;

            foreach (object o in dict.Keys)
            {
                if (!shouldContinue)
                    break;

                // Write key value pairs next to each other.
                shouldContinue = keyResolver.Write(this, o);

                if (!shouldContinue)
                    break;

                shouldContinue = valueResolver.Write(this, dict[o]);
            }

            return shouldContinue;
        }

        private bool _Internal_TryReadDictionary<T>(in Type keyType, in Type valueType, out T output) where T : new()
        {
            output = default;

            IResolver keyResolver = GetResolver(keyType);
            IResolver valueResolver = GetResolver(valueType);

            if (keyResolver == null || valueResolver == null)
            {
                Serilog.Log.Warning($"BytePayload#TryReadDictionary(): Cannot resolve IDictionary<{keyType.Name}, {valueType.Name}>! Do you have resolvers for both types?");
                return false;
            }

            int read = Stream.ReadByte();

            if (read == -1)
                return false;

            byte header = (byte) read;

            if (header == IResolver.NullArrayHeader
                || header == IResolver.EmptyArrayHeader)
            {
                output = header == IResolver.NullArrayHeader ? default : new();
                return true;
            }

            IResolver._Internal_ReadHeaderAndGetLength(this, header, out int length);

            T dictOutput = new();

            for (int i = 0; i < length; i++)
            {
                if (keyResolver.Read(this, out object key).IsSuccess() && valueResolver.Read(this, out object value).IsSuccess())
                {
                    ((IDictionary) dictOutput).Add(key, value);
                    continue;
                }

                // Something went wrong reading a key or value.
                break;
            }

            output = dictOutput;
            return ((IDictionary)dictOutput).Count == length;
        }

        public virtual bool TryReadDictionary<TKey, TValue>(out Dictionary<TKey, TValue> output)
        {
            output = null;

            Resolver<TKey> keyResolver = GetResolver<TKey>();
            Resolver<TValue> valueResolver = GetResolver<TValue>();

            if (keyResolver == null || valueResolver == null)
            {
                Serilog.Log.Warning($"BytePayload#TryReadDictionary(): Cannot resolve IDictionary<{typeof(TKey).Name}, {typeof(TValue).Name}>! Do you have resolvers for both types?");
                return false;
            }

            int read = Stream.ReadByte();

            if (read == -1)
                return false;

            byte header = (byte)read;

            if (header == IResolver.NullArrayHeader || header == IResolver.EmptyArrayHeader)
            {
                output = header == IResolver.NullArrayHeader ? default : new();
                return true;
            }

            IResolver._Internal_ReadHeaderAndGetLength(this, header, out int length);

            Dictionary<TKey, TValue> dictOutput = new();

            for (int i = 0; i < length; i++)
            {
                if (keyResolver.Read(this, out TKey key).IsSuccess() && valueResolver.Read(this, out TValue value).IsSuccess())
                {
                    dictOutput.Add(key, value);
                    continue;
                }

                // Something went wrong reading a key or value.
                break;
            }

            output = dictOutput;
            return dictOutput.Count == length;
        }

        /// <summary>
        /// Tries to write an object of unknown type.
        /// Returns true if a successful companion write function was called or
        /// false if unable to find the correct function.
        /// </summary>

        public virtual bool TryWrite(object input)
        {
            Type type = input.GetType();

            if (type.IsArray)
                type = type.GetElementType();

            if (Type.GetTypeCode(type) == TypeCode.String)
            {
                // Cannot just write a string willy nilly
                Serilog.Log.Warning("BytePayload#TryWrite(): You must specify if this is a String8 or String16!");
                return false;
            }

            IResolver resolver = GetResolver(type);
            return resolver?.Write(this, input) == true;
        }

        /// <summary>
        /// Tries to write an object of unknown type.
        /// Returns true if a successful companion write function was called or
        /// false if unable to find the correct function.
        /// </summary>

        public virtual bool TryWrite<T>(T input)
        {
            Type type = input.GetType();

            if (Type.GetTypeCode(type) == TypeCode.String || (type.IsArray && Type.GetTypeCode(type.GetElementType()) == TypeCode.String))
            {
                // Cannot just write a string willy nilly
                Serilog.Log.Warning("BytePayload#TryWrite(): You must specify if this is a String8 or String16!");
                return false;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                return TryWriteDictionary((IDictionary) input, type.GetGenericArguments()[0], type.GetGenericArguments()[1]);

            Resolver<T> resolver = GetResolver<T>();
            return resolver?.Write(this, input) == true;
        }

        /// <summary>
        /// Locates the proper read function for the specified type, calls it,
        /// and returns the result.
        /// If no companion function is located to call, null is returned.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Result of companion #Read?() function or null</returns>

        public virtual bool TryRead<T>(out T output) where T : new()
        {
            Type type = typeof(T);
            output = default;

            if (Type.GetTypeCode(type) == TypeCode.String || (type.IsArray && Type.GetTypeCode(type.GetElementType()) == TypeCode.String))
            {
                // Cannot just write a string willy nilly
                Serilog.Log.Warning("BytePayload#TryRead(): You must specify if this is a String8 or String16!");
                return false;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                return _Internal_TryReadDictionary(type.GetGenericArguments()[0], type.GetGenericArguments()[1], out output);

            Resolver<T> resolver = GetResolver<T>();
            return resolver?.Read(this, out output).IsSuccess() == true; 
        }

        /// <summary>
        /// Packages the data into a byte array to be send on the wire.
        /// <br/>It's recommended to only call this when you're completely done writing data.
        /// </summary>
        /// <returns></returns>

        public virtual byte[] Pack()
        {
            if (Stream == null)
                return null;

            if (PackedData == null)
            {
                this.PackedData = new byte[Stream.Length];
                Stream.WriteTo(PackedData);
            }

            return PackedData;
        }

        public virtual void Reset()
        {

            bool hadStream = Stream != null;

            Dispose();

            if (hadStream)
                EnsureReadyToWrite();
        }

        /// <summary>
        /// Disposes of the internal <see cref="RecyclableMemoryStream"/>.
        /// <br/>DO NOT call unless you're sure you're done reading and writing from this payload.
        /// </summary>

        public virtual void Dispose()
        {
            Stream?.Dispose();
            Stream = null;

            PackedData = null;
        }

        /// <summary>
        /// Reads a string in the encoding specified by <see cref="Encoder"/>. <br/>
        /// Only call this if you know the exact length of the string!
        /// </summary>
        /// <returns></returns>

        public string ReadString(int length)
        {
            Span<byte> buffer = stackalloc byte[length];
            int read = Stream.Read(buffer);

            return Encoder.GetString(buffer);
        }

        /// <summary>
        /// Returns the size of this payload in kilobytes.
        /// </summary>
        /// <returns></returns>

        public double GetSizeKB()
        {
            float length = (Stream != null) ? Stream.Length : 0;
            return (length * 0.001).Round(3);
        }

        /// <summary>
        /// Ensures the internal <see cref="RecyclableMemoryStream"/> is ready to be written to.
        /// </summary>

        public virtual void EnsureReadyToWrite()
        {
            Stream ??= ObtainStream();
        }

        /// <summary>
        /// Closes the existing stream (if necessary) and obtains
        /// a new one.
        /// </summary>

        public virtual void ResetStream()
        {
            Stream?.Close();
            Stream = ObtainStream();
        }

    }

}
