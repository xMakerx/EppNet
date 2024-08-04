///////////////////////////////////////////////////////
/// Filename: ActionExtensions.cs
/// Date: August 3, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Threading.Tasks;

namespace EppNet.Utilities
{

    public static class ActionExtensions
    {

        public static void GlobalInvoke<T>(this Action<T> action, T parameter)
        {
            SynchronizationContext context = SynchronizationContext.Current;

            if (context != null)
                // Call on the main thread synchronously
                context.Send(_ => action.Invoke(parameter), null);
            else
                Task.Factory.StartNew(() => action.Invoke(parameter));
        }

    }

}