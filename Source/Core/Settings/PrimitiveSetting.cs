///////////////////////////////////////////////////////
/// Filename: PrimitiveSetting.cs
/// Date: April 14, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

using Notify = EppNet.Utilities.LoggingExtensions;

namespace EppNet.Core.Settings
{
    public class PrimitiveSetting<TValue> : ISetting<TValue> where TValue : struct, IComparable,
        IFormattable, IConvertible, IComparable<TValue>, IEquatable<TValue>
    {

        /// <summary>
        /// Called when the value of this setting changes.
        /// <br/>See <see cref="SettingValueChangedEvent{TValue}"/> for more information.
        /// </summary>
        public event Action<SettingValueChangedEvent<TValue>> OnValueChanged;

        public readonly string Key;
        public TValue Value { protected set; get; }

        // For range limits
        private TValue _lowerBounds;
        private TValue _upperBounds;

        public PrimitiveSetting(string key)
        {
            this.Key = key;
            this.Value = default;
        }

        public PrimitiveSetting(string key, TValue defaultValue)
        {
            this.Key = key;
            this.Value = defaultValue;
        }

        public string GetKey() => Key;

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

        public Tuple<TValue, TValue> GetAcceptableRange() => new(_lowerBounds, _upperBounds);

        /// <summary>
        /// Checks if the provided <see cref="TValue"/> is acceptable<br/>
        /// i.e. in the acceptable range (if one was provided)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        public bool IsAcceptable(TValue value)
        {
            bool hasRange = !_lowerBounds.Equals(_upperBounds);
            return (_lowerBounds.CompareTo(value) <= 0 && _upperBounds.CompareTo(value) >= 0)
                || !hasRange;
        }

        protected bool _Internal_TrySetValue(TValue value)
        {
            // If we have equivalent values, no need to change it.
            if (Value.Equals(value))
                return false;

            if (!IsAcceptable(value))
            {
                Notify.Warn($"Setting {Key} value must be between [{_lowerBounds}, {_upperBounds}], given {value}!");
                return false;
            }

            TValue prevValue = this.Value;
            this.Value = value;

            // Event is explicitly created here for readibility reasons.
            SettingValueChangedEvent<TValue> evt = new(this, prevValue, value);
            OnValueChanged?.Invoke(evt);

            return true;
        }

        public bool SetValue(TValue value) => _Internal_TrySetValue(value);

        public TValue GetValue() => Value;

        public bool TryApply()
        {
            throw new NotImplementedException();
        }
    }
}
