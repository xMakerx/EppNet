///////////////////////////////////////////////////////
/// Filename: ISetting.cs
/// Date: April 14, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Core.Settings
{

    public interface ISetting<TValue>
    {

        string GetKey();

        bool SetValue(TValue value);

        TValue GetValue();

        bool IsAcceptable(TValue value);

        bool TryApply();

    }

}
