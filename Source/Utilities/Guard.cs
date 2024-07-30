///////////////////////////////////////////////////////
/// Filename: Guard.cs
/// Date: July 29, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Logging;
using EppNet.Node;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EppNet.Utilities
{

    /// <summary>
    /// This was inspired by Serilog's Guard and Guard.Against methods<br/>
    /// Return of false means that the guard was tripped
    /// </summary>
    public static class Guard
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AgainstDefault<THost, TArg>(this THost host, TArg arg) where THost : class where TArg : struct => AgainstDefault(host, arg, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AgainstDefault<THost, TArg>(this THost host, TArg arg, TemplatedMessage? tmpMsg, bool fatal = false) where THost : class where TArg : struct
        {
            TArg d = default;

            if (!d.Equals(arg))
                return true;

            return _DoErrorHandling(host, arg, tmpMsg, fatal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AgainstNull<THost, TArg>(this THost host, TArg arg) where THost : class where TArg : class => AgainstNull(host, arg, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AgainstNull<THost, TArg>(this THost host, TArg arg, TemplatedMessage? tmpMsg, bool fatal = false) where THost : class where TArg : class
        {

            if (arg is not null)
                return true;

            return _DoErrorHandling(host, arg, tmpMsg, fatal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AgainstNull(params object[] arguments)
        {
            INodeDescendant nodeDescendant = null;

            foreach (object arg in arguments)
            {
                if (arg != null)
                {
                    if (arg is INodeDescendant n)
                        nodeDescendant = n;

                    continue;
                }

                var exception = new ArgumentNullException(nameof(arg));

                if (nodeDescendant != null)
                    nodeDescendant.Node.HandleException(exception);
                else
                    throw exception;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AgainstNull<T>([NotNull] T argument) where T : class
        {
            if (argument is null)
                throw new ArgumentNullException(nameof(argument));

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool _DoErrorHandling<THost, TArg>(THost host, TArg arg, TemplatedMessage? tmpMsg, bool fatal = false) where THost : class
        {
            ArgumentNullException exp = new(tmpMsg.HasValue ? tmpMsg.Value.Message : nameof(arg));

            if (tmpMsg.HasValue && host is ILoggable loggable)
            {
                if (fatal)
                    loggable.Notify.Fatal(tmpMsg.Value, exp);
                else
                    loggable.Notify.Error(tmpMsg.Value, exp);

            }
            else if (host is INodeDescendant nDesc)
                nDesc.Node.HandleException(exp);

            return false;
        }


    }

}
