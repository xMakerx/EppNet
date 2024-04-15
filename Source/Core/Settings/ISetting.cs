///////////////////////////////////////////////////////
/// Filename: ISetting.cs
/// Date: April 14, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Core.Settings
{

    public interface ISetting<TValue>
    {
        TValue Value { set; get; }

        bool IsAcceptable(TValue value);

        bool TryApply();

    }

}
