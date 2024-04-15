///////////////////////////////////////////////////////
/// Filename: ISetting.cs
/// Date: April 14, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Core.Settings
{

    public interface ISetting<TValue>
    {

        string Key { get; }
        TValue Value { set; get; }

        /// <summary>
        /// If this setting should be written to the
        /// configuration file.
        /// </summary>
        /// <returns>Defaults to true</returns>

        bool WritesToFile { set; get; }

        bool IsAcceptable(TValue value);

        bool TryApply();

    }

}
