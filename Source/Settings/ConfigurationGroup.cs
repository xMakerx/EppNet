///////////////////////////////////////////////////////
/// Filename: ConfigurationGroup.cs
/// Date: April 15, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text.Json;

using EppNet.Logging;

namespace EppNet.Settings
{
    public class ConfigurationGroup : Writeable, ILoggable
    {
        public ILoggable Notify { get => this; }

        public bool IsRoot => string.IsNullOrEmpty(Key);

        public List<Writeable> Items { get => _items; }

        protected List<Writeable> _items;

        public ConfigurationGroup(string key) : base(key)
        {
            this.WritesToFile = true;
            this._items = new();
        }

        public ConfigurationGroup(Configuration config, string key) : this(key)
        {
            this.Configuration = config;
        }

        public bool Add(Writeable item)
        {
            // To add an item to this group, the item must:
            // 1) Not be null
            // 2) Not have a parent
            // 3) Not be this group

            if (item == null || item.Parent != null || item == this)
                return false;

            if (_items.Contains(item))
                return false;

            _items.Add(item);
            item.Configuration = this.Configuration;
            return true;
        }

        /// <summary>
        /// Removes the specified <see cref="Writeable"/> from this group
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>

        public bool Remove(Writeable item)
        {
            bool removed = _items.Remove(item);
            item.Parent = null;
            return removed;
        }

        /// <summary>
        /// Clears all writeables associated with this group
        /// </summary>

        public void Clear()
        {
            int count = Items.Count;

            while (Items.Count > 0)
                Remove(Items[0]);

            if (count != Items.Count)
                // We've cleared out some items.
                // TODO: Push update to settings server
                return;
        }

        public override Writeable Clone()
        {
            ConfigurationGroup clone = new(Key)
            {
                WritesToFile = this.WritesToFile
            };

            foreach (var item in Items)
                clone.Items.Add(item.Clone());

            return clone;
        }

        internal override void Write(Utf8JsonWriter writer)
        {
            if (!WritesToFile)
                return;

            if (!IsRoot)
                writer.WriteStartObject(Key);

            foreach (var item in Items)
            {
                try
                {
                    // The writeable itself will just return if it isn't
                    // supposed to be written.
                    item.Write(writer);
                }
                catch (Exception e)
                {
                    // Something went wrong while trying to write this thing.
                    // Don't break the entire configuration because of it.
                    Notify.Fatal($"Failed to write Writeable \"{item.Key}\". Exception: \"{e.Message}\".");
                }
            }

            if (!IsRoot)
                writer.WriteEndObject();
        }

    }
}
