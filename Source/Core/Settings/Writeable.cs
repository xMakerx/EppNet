///////////////////////////////////////////////////////
/// Filename: Writeable.cs
/// Date: April 15, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System.Text.Json;

namespace EppNet.Core.Settings
{

    public abstract class Writeable
    {

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
        public Writeable Parent
        {
            internal set
            {
                if (value != this)
                    _parent = value;
            }

            get => _parent;
        }

        private Writeable _parent;

        protected Writeable(string key)
        {
            this.Key = key;
            this._parent = null;
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