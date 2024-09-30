///////////////////////////////////////////////////////
/// Filename: TimeExtensions.cs
/// Date: July 30, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Node;

using System;
using System.Runtime.CompilerServices;

namespace EppNet.Time
{

    public static class TimeExtensions
    {

        /// <summary>
        /// Fetches the current monotonic time from the ENet library in milliseconds
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan MonoTime()
            => TimeSpan.FromMilliseconds(ENet.Library.Time);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan Time(this INodeDescendant obj)
            => obj.Node.Time;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan DetermineCurrentTime(params object[] objects)
        {

            foreach (var obj in objects)
            {
                if (obj is not null && obj is INodeDescendant nDesc)
                    return nDesc.Time();
            }

            return TimeSpan.FromMilliseconds(ENet.Library.Time);
        }

    }

}