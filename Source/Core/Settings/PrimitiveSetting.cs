///////////////////////////////////////////////////////
/// Filename: PrimitiveSetting.cs
/// Date: April 14, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Text.Json;

using Notify = EppNet.Logging.LoggingExtensions;

namespace EppNet.Core.Settings
{
    public class PrimitiveSetting<TValue> : Writeable, ISetting<TValue> where TValue : struct, IComparable,
        IFormattable, IConvertible, IComparable<TValue>, IEquatable<TValue>
    {

        /// <summary>
        /// Called when the value of this setting changes.
        /// <br/>See <see cref="SettingValueChangedEvent{TValue}"/> for more information.
        /// </summary>
        public event Action<SettingValueChangedEvent<TValue>> OnValueChanged;

        public TValue Value
        {
            set => _Internal_TrySetValue(value);
            get => _value;
        }

        protected TValue _value;

        // For range limits
        private TValue _lowerBounds;
        private TValue _upperBounds;

        public PrimitiveSetting(string key) : base(key)
        {
            this._value = default;
        }

        public PrimitiveSetting(string key, TValue value) : base(key)
        {
            this._value = value;
        }

        /// <summary>
        /// Sets the acceptable range for values<br/>
        /// Values must be equal to or greater than the lower bounds and
        /// less than or equal to the upper bounds.<br/>
        /// (a less than or equal to n AND n less than or equal to b) OR [a, b])
        /// </summary>

        public void SetAcceptableRange(TValue lowerBounds, TValue upperBounds)
        {
            int lComp = lowerBounds.CompareTo(upperBounds);
            int uComp = upperBounds.CompareTo(lowerBounds);

            if (Notify.AssertTrueOrFatal((lComp < 0 && uComp > 0) || lComp == uComp,
                $"Setting {Key} was provided an invalid range of [{lowerBounds}, {upperBounds}]"))
            {
                // Set the bounds
                this._lowerBounds = lowerBounds;
                this._upperBounds = upperBounds;
                Notify.Debug($"Setting {Key} range set to [{lowerBounds}, {upperBounds}]");
            }
        }

        /// <summary>
        /// Returns a tuple representing the lower and upper bounds for
        /// acceptable values.<br/>
        /// (a less than or equal to n AND n less than or equal to b) OR [a, b])
        /// </summary>
        /// <returns>Tuple with lower and upper bounds</returns>

        public Tuple<TValue, TValue> GetAcceptableRange() => new(_lowerBounds, _upperBounds);

        /// <summary>
        /// Checks if the provided <see cref="TValue"/> is acceptable<br/>
        /// i.e. in the acceptable range (if one was provided)<br/>
        /// See <see cref="GetAcceptableRange"/> for more information.
        /// </summary>

        public bool IsAcceptable(TValue value)
        {
            bool hasRange = !_lowerBounds.Equals(_upperBounds);
            return (_lowerBounds.CompareTo(value) <= 0 && _upperBounds.CompareTo(value) >= 0)
                || !hasRange;
        }

        public bool TryApply()
        {
            throw new NotImplementedException();
        }

        public override Writeable Clone()
        {
            PrimitiveSetting<TValue> clone = new(Key, Value)
            {
                WritesToFile = this.WritesToFile
            };
            clone.SetAcceptableRange(_lowerBounds, _upperBounds);
            return clone;
        }

        protected bool _Internal_TrySetValue(TValue value)
        {
            // If we have equivalent values, no need to change it.
            if (_value.Equals(value))
                return false;

            if (!IsAcceptable(value))
            {
                Notify.Warn($"Setting {Key} value must be between [{_lowerBounds}, {_upperBounds}], given {value}!");
                return false;
            }

            TValue prevValue = this._value;
            this._value = value;

            // Event is explicitly created here for readibility reasons.
            SettingValueChangedEvent<TValue> evt = new(this, prevValue, value);
            OnValueChanged?.Invoke(evt);

            return true;
        }

        internal override void Write(Utf8JsonWriter writer)
        {
            if (!WritesToFile)
                return;

            writer.WritePropertyName(Key);
            writer.WriteRawValue(Value.ToString(), skipInputValidation: true);
        }

    }
}
