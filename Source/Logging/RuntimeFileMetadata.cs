//////////////////////////////////////////////
/// <summary>
/// Filename: RuntimeFileMetadata.cs
/// Date: January 14, 2024
/// Author: Maverick Liberty
/// </summary>
//////////////////////////////////////////////

using Serilog.Events;

using System.Collections.Generic;
using System.Text;

namespace EppNet.Logging
{

    public class RuntimeFileMetadata
    {

        private const string Unknown = "Unknown";
        private const string Missing = "??";

        public static readonly RuntimeFileMetadata Default = new();

        private static Dictionary<string, RuntimeFileMetadata> _filepath2Data = new()
        {
            { string.Empty, Default }
        };

        private static Dictionary<string, RuntimeFileMetadata> _filename2Data = new()
        {
            { Unknown, Default }
        };

        private static void _Internal_CacheEntry(RuntimeFileMetadata metadata)
        {
            _filename2Data[metadata.Filename] = metadata;
            _filepath2Data[metadata.Filepath] = metadata;
        }

        public static void ClearCache()
        {
            _filename2Data.Clear();
            _filepath2Data.Clear();
        }

        public static bool TryGetMetadataByPath(string filepath, out RuntimeFileMetadata metadata) => _filepath2Data.TryGetValue(filepath, out metadata);

        public static bool TryGetMetadataByName(string filename, out RuntimeFileMetadata metadata) => _filename2Data.TryGetValue(filename, out metadata);

        public static bool GetMetadataFromName(string filename, out RuntimeFileMetadata metadata, bool cacheIfNecessary = true)
        {
            if (string.IsNullOrEmpty(filename))
            {
                metadata = RuntimeFileMetadata.Default;
                return false;
            }

            // Check if we've already determined the filename
            if (TryGetMetadataByName(filename, out metadata))
                return true;

            metadata = new(filename, Missing);

            if (cacheIfNecessary)
                _Internal_CacheEntry(metadata);

            return false;
        }

        public static bool GetMetadataFromPath(string filepath, out RuntimeFileMetadata metadata, bool cacheIfNecessary = true)
        {

            // This is okay as we have some default values for null or whitespace.
            if (string.IsNullOrWhiteSpace(filepath))
                filepath = string.Empty;

            // Check if we've already determined the filename
            if (TryGetMetadataByPath(filepath, out metadata))
                return true;

            // For some UNKNOWN reason Path#GetFileNameWithoutExtension
            // isn't working so I had to roll my own solution.

            StringBuilder builder = new StringBuilder();
            bool addChars = false;

            for (int i = filepath.Length - 1; i > 0; i--)
            {
                char c = filepath[i];

                // We only care about the file name.
                if (c == '\\' || c == '/')
                    break;

                // We want to ignore the .cs or any other extension
                if (!addChars)
                {
                    if (c == '.')
                        addChars = true;

                    continue;
                }

                // We want to add this character to the beginning
                builder.Insert(0, c);
            }

            string filename = builder.ToString();

            // Create a new RuntimeFileMetadata
            metadata = new(filename, filepath);

            if (cacheIfNecessary)
                _Internal_CacheEntry(metadata);

            return false;
        }

        public string Filename { protected set; get; }
        public string Filepath { protected set; get; }

        /// <summary>
        /// A <see cref="LogEventLevel"/> of NULL means that we're
        /// using the global log event level rather than our own.
        /// </summary>
        public LogLevelFlags LogLevel { set; get; }

        private RuntimeFileMetadata() : this(Unknown, Missing) { }

        private RuntimeFileMetadata(string filename, string filepath)
        {
            this.Filename = filename;
            this.Filepath = filepath;
            this.LogLevel = LogLevelFlags.InfoWarnFatal;
        }

        public bool IsDefault() => Filename == Unknown && Filepath == Missing;


    }

}
