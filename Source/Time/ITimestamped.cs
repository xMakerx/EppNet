///////////////////////////////////////////////////////
/// Filename: ITimestamped.cs
/// Date: September 30, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;

using System;

namespace EppNet.Time
{
    
    public interface ITimestamped
    {

        Timestamp Timestamp { get; }

    }

}
