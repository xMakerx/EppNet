///////////////////////////////////////////////////////
/// Filename: Writeable.cs
/// Date: April 15, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System.Text.Json;

namespace EppNet.Settings
{

    public abstract class Writeable : IConfigurationDescendant
    {

        public Configuration Configuration { internal set; get; }

        /// <summary>
        /// The name of this writeable
        /// </summary>

        public string Key { get; }

        /// <summary>
        /// If this setting should be written to the
        /// configuration file.
        /// </summary>
        /// <returns>Defaults to true</returns>

        public bool WritesToFile
        {
            set
            {
                if (value != _writesToFile)
                {
                    // TODO: Push update to settings server
                    _writesToFile = value;
                }
            }

            get => _writesToFile;
        }

        protected bool _writesToFile;
        public Writeable Parent { internal set; get; }

        protected Writeable(string key)
        {
            this.Key = key;
            this.Parent = null;
            this._writesToFile = true;
        }

        protected Writeable(string key, Writeable parent) : this(key)
        {
            this.Parent = parent;
        }

        internal abstract void Write(Utf8JsonWriter writer);

        public abstract Writeable Clone();

    }

}