///////////////////////////////////////////////////////
/// Filename: SettingValueChangedEvent.cs
/// Date: April 14, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Core.Settings
{
    public struct SettingValueChangedEvent<TValue>
    {

        public readonly ISetting<TValue> Setting;
        public readonly TValue PreviousValue;
        public readonly TValue Value;

        public SettingValueChangedEvent(ISetting<TValue> setting, TValue previousValue, TValue value)
        {
            this.Setting = setting;
            this.PreviousValue = previousValue;
            this.Value = value;
        }
    }

}

