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
        public static bool IsNotDefault<THost, TArg>(this THost host, TArg arg, [CallerMemberName] string callerMemberName = null) where THost : class where TArg : struct
            => IsNotDefault(host, arg, null, false, callerMemberName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotDefault<THost, TArg>(this THost host, TArg arg, string message, bool fatal = false, [CallerMemberName] string callerMemberName = null) where THost : class where TArg : struct
        {
            return IsNotDefault(host, arg, new TemplatedMessage(message), fatal, callerMemberName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotDefault<THost, TArg>(this THost host, TArg arg, TemplatedMessage tmpMsg, bool fatal = false, [CallerMemberName] string callerMemberName = null) where THost : class where TArg : struct
        {
            TArg d = default;

            if (!d.Equals(arg))
                return true;

            return _Internal_DoErrorHandling(host, tmpMsg, fatal, callerMemberName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNull<THost, TArg>(this THost host, TArg arg, [CallerMemberName] string callerMemberName = null) where THost : class where TArg : class
            => IsNotNull(host, arg, null, callerMemberName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNull<THost, TArg>(this THost host, TArg arg, string message, [CallerMemberName] string callerMemberName = null) where THost : class where TArg : class
        {
            return IsNotNull(host, arg, new TemplatedMessage(message), false, callerMemberName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNull<THost, TArg>(this THost host, TArg arg, TemplatedMessage tmpMsg, bool fatal = false, [CallerMemberName] string callerMemberName = null) where THost : class where TArg : class
        {

            if (arg is not null)
                return true;

            return _Internal_DoErrorHandling(host, tmpMsg, fatal, callerMemberName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNull<THost>(this THost host, [CallerMemberName] string callerMemberName = null, params object[] arguments) where THost : class
            => IsNotNull(host, null, false, arguments, callerMemberName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNull<THost>(this THost host, TemplatedMessage tmpMsg, bool fatal = false, [CallerMemberName] string callerMemberName = null, params object[] arguments) where THost : class
        {

            foreach (object arg in arguments)
            {
                if (arg is null)
                {
                    _Internal_DoErrorHandling(host, tmpMsg, fatal, callerMemberName);
                    return false;
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool _Internal_DoErrorHandling<THost>(THost host, TemplatedMessage tmpMsg, bool fatal = false, [CallerMemberName] string callerMemberName = null)
            where THost : class
        {
            ArgumentNullException exp = new();

            if (!string.IsNullOrEmpty(tmpMsg.Message) && host is ILoggable loggable)
            {
                if (fatal)
                    loggable.Notify.Fatal(tmpMsg, exp, callerMemberName: callerMemberName);
                else
                    loggable.Notify.Error(tmpMsg, exp, callerMemberName: callerMemberName);

            }
            else if (host is INodeDescendant nDesc)
                nDesc.Node.HandleException(exp);
            else
                throw exp;

            return false;
        }


    }

}
