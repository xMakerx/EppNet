///////////////////////////////////////////////////////
/// Filename: Configuration.cs
/// Date: April 15, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Logging;
using EppNet.Node;
using EppNet.Utilities;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace EppNet.Settings
{

    public class Configuration : INodeDescendant, ILoggable
    {

        public NetworkNode Node { get; }
        public ILoggable Notify { get => this; }

        /// <summary>
        /// The path of the directory to save the configuration file</br>
        /// Defaults to AppDomain.CurrentDomain.BaseDirectory + "\config"
        /// </summary>
        public string ConfigurationPath
        {
            set
            {
                if (Node.Started)
                {
                    Notify.Error("Cannot update config file details while running!");
                    return;
                }

                // Let's sanitize the input
                string input = value;

                if (string.IsNullOrEmpty(input))
                {
                    Notify.Error("Invalid input!");
                    return;
                }

                int lastDotIndex = input.LastIndexOf('.');
                int lastPathSepIndex = StringUtilities.GetLastPathSeparatorIndex(input);
                bool endsWithSeparator = lastPathSepIndex == input.Length - 1;

                // If we're provided a path that includes a file name and extension, let's fix it
                if (lastPathSepIndex != -1 && lastDotIndex > lastPathSepIndex)
                {
                    input = input.Substring(0, lastPathSepIndex);
                    endsWithSeparator = true;
                }

                if (!endsWithSeparator)
                    input += '/';

                _configPath = input;
            }

            get => _configPath;
        }

        /// <summary>
        /// The configuration filename<br/>
        /// If this node is named: <see cref="Name"/> + .json<br/>
        /// Otherwise, "config.json"
        /// </summary>
        public string ConfigurationFilename
        {
            set
            {
                if (Node.Started)
                {
                    Notify.Error("Cannot update config file details while running!");
                    return;
                }

                // Let's sanitize the input
                string input = value;

                if (string.IsNullOrEmpty(input))
                {
                    Notify.Error("Invalid input!");
                    return;
                }

                if (!input.EndsWith(".json"))
                    input += ".json";

                _configFilename = input;
            }

            get => _configFilename;
        }

        public string ConfigurationFullPath
        {
            get => ConfigurationPath + ConfigurationFilename;
        }

        public ConfigurationGroup MainGroup { get; }

        public JsonWriterOptions FileOptions = new()
        {
            Indented = true
        };

        /// <summary>
        /// If dirty, we need to rewrite this config to file.
        /// </summary>
        public bool Dirty { internal set; get; }
        public Task AsyncWriteTask { private set; get; }

        // Internal strings to hold configuration file data.
        internal string _configPath, _configFilename;

        public Configuration([NotNull] NetworkNode node)
        {
            Guard.AgainstNull(node);
            this.Node = node;
            this.MainGroup = new(this, null);
            this.ConfigurationPath = AppDomain.CurrentDomain.BaseDirectory + @"\config";

            if (string.IsNullOrEmpty(Node.Name))
                this.ConfigurationFilename = $"{nameof(Node.Distro)}.json";
            else
                this.ConfigurationFilename = $"{Node.Name}.json";

            this.Dirty = false;
            this.AsyncWriteTask = null;
        }

        public bool Add(Writeable item)
            => MainGroup.Add(item);

        public void Write()
        {
            using FileStream stream = File.OpenWrite(ConfigurationFullPath);
            using Utf8JsonWriter writer = new(stream, FileOptions);

            writer.WriteStartObject();
            MainGroup.Write(writer);
            writer.WriteEndObject();
            writer.Flush();
        }

        public async void WriteAsync()
        {
            AsyncWriteTask = Task.Run(Write);
            await AsyncWriteTask;

            AsyncWriteTask = null;
        }

    }
}
