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

        private static Dictionary<string, RuntimeFileMetadata> _filepath2Data = new()
        {
            { string.Empty, new() }
        };

        private static Dictionary<string, RuntimeFileMetadata> _filename2Data = new()
        {
            { Unknown, new() }
        };

        private static void _Internal_CacheEntry(string filename, string filepath)
        {
            RuntimeFileMetadata metadata = new RuntimeFileMetadata(filename, filepath);
            _filename2Data[filename] = metadata;
            _filepath2Data[filepath] = metadata;
        }

        public static void ClearCache()
        {
            _filename2Data.Clear();
            _filepath2Data.Clear();
        }

        public static bool TryGetMetadata(string filepath, out RuntimeFileMetadata metadata)
        {
            return _filepath2Data.TryGetValue(filepath, out metadata);
        }

        public static string GetFilenameFromPath(string filepath, bool cacheIfNecessary = true)
        {
            // This is okay as we have some default values for null or whitespace.
            if (string.IsNullOrWhiteSpace(filepath))
                filepath = string.Empty;

            // Check if we've already determined the filename
            if (TryGetMetadata(filepath, out RuntimeFileMetadata metadata))
                return metadata.Filename;

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

            if (cacheIfNecessary)
                _Internal_CacheEntry(filename, filepath);

            return filename;
        }

        public string Filename { protected set; get; }
        public string Filepath { protected set; get; }

        /// <summary>
        /// A <see cref="LogEventLevel"/> of NULL means that we're
        /// using the global log event level rather than our own.
        /// </summary>
        public LogEventLevel? LogLevel { set; get; }

        private RuntimeFileMetadata() : this(Unknown, Missing) { }

        private RuntimeFileMetadata(string filename, string filepath)
        {
            this.Filename = filename;
            this.Filepath = filepath;
            this.LogLevel = null;
        }

        public bool IsDefault() => Filename == Unknown && Filepath == Missing;


    }

}
