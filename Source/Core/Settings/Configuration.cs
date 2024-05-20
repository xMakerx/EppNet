///////////////////////////////////////////////////////
/// Filename: Configuration.cs
/// Date: April 15, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System.IO;
using System.Text.Json;

namespace EppNet.Core.Settings
{

    internal class Configuration
    {

        public ConfigurationGroup MainGroup { get => _mainGroup; }

        public JsonWriterOptions FileOptions = new()
        {
            Indented = true
        };

        protected ConfigurationGroup _mainGroup = new(null);

        public bool Add(Writeable item) => _mainGroup.Add(item);

        public void Write()
        {

            using FileStream stream = File.OpenWrite(SettingsService.GetFullFilePath());
            using Utf8JsonWriter writer = new(stream, FileOptions);

            writer.WriteStartObject();
            MainGroup.Write(writer);
            writer.WriteEndObject();
            writer.Flush();
        }

    }
}
