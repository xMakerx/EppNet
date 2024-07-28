//////////////////////////////////////////////
/// <summary>
/// Filename: ILoggable.cs
/// Date: July 10, 2024
/// Author: Maverick Liberty
/// </summary>
//////////////////////////////////////////////

using EppNet.Node;

using Serilog.Events;

using System;
using System.Runtime.CompilerServices;

namespace EppNet.Logging
{

    public interface ILoggable
    {

        public ILoggable Notify { get; }

        protected internal void _Internal_SetMetadata(RuntimeFileMetadata metadata) { }

        protected internal RuntimeFileMetadata _Internal_GetMetadata() => null;
    }

    public static class ILoggableExtensions
    {

        public static string ResolveMemberName(string callerMemberName)
            => callerMemberName == ".ctor" ? "ctor" : callerMemberName;

        /// <summary>
        /// Sets the enabled <see cref="LogLevelFlags"/>
        /// </summary>
        /// <param name="loggable"></param>
        /// <param name="level"></param>

        public static void SetLogLevel(this ILoggable loggable, LogLevelFlags flags)
        {
            RuntimeFileMetadata metadata = _Internal_CreateOrGetMetadata(loggable);
            metadata.LogLevel = flags;
        }

        /// <summary>
        /// Enables the specified <see cref="LogLevelFlags"/> for this <see cref="ILoggable"/>
        /// </summary>
        /// <param name="loggable"></param>
        /// <param name="flag"></param>

        public static void EnableLogLevel(this ILoggable loggable, LogLevelFlags flag)
        {
            RuntimeFileMetadata metadata = _Internal_CreateOrGetMetadata(loggable);
            metadata.LogLevel |= flag;
        }

        /// <summary>
        /// Gets the <see cref="LogLevelFlags"/> for this loggable.<br/>
        /// If no <see cref="LogEventLevel"/> was set, the following takes precendence:<br/>
        /// - If Loggable is <see cref="INodeDescendant"/>, uses the <see cref="LogEventLevel"/> of the associated <see cref="NetworkNode"/><br/>
        /// </summary>
        /// <param name="loggable"></param>
        /// <returns></returns>

        public static LogLevelFlags GetLogLevel(this ILoggable loggable)
        {
            RuntimeFileMetadata metadata = _Internal_CreateOrGetMetadata(loggable);
            return metadata.LogLevel;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Msg(this ILoggable loggable, LogLevelFlags flags, TemplatedMessage msgData,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null)
        {

            LogEventLevel? level = flags.ToLevel();

            if (!level.HasValue)
            {
                // Invalid usage of LogLevelFlags. Let's send an error out
                TemplatedMessage errMsgData = new(
                    "Tried to log a message with more than one \"LogLevelFlags\" set. Only one AT most is allowed. Message: {message}",
                    msgData.Message);

                loggable._Internal_DoMsg(LogEventLevel.Error, errMsgData, exception, callerMemberName);
                return false;
            }

            return loggable._Internal_DoMsg(level.Value, msgData, exception, callerMemberName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Msg(this ILoggable loggable, LogEventLevel level, string message,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable._Internal_DoMsg(level, new(message), exception, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Msg(this ILoggable loggable, LogEventLevel level, string message,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null,
            params object[] objects)
            => loggable._Internal_DoMsg(level, new(message, objects), exception, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Fatal(this ILoggable loggable, TemplatedMessage msgData,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable._Internal_DoMsg(LogEventLevel.Fatal, msgData, exception, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Fatal(this ILoggable loggable, string message,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null) 
            => loggable.Msg(LogEventLevel.Fatal, message, exception, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Error(this ILoggable loggable, string message,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.Msg(LogEventLevel.Error, message, exception, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Error(this ILoggable loggable, TemplatedMessage msgData,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable._Internal_DoMsg(LogEventLevel.Error, msgData, exception, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Warning(this ILoggable loggable, string message,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.Msg(LogEventLevel.Warning, message, exception, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Warning(this ILoggable loggable, TemplatedMessage msgData,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable._Internal_DoMsg(LogEventLevel.Warning, msgData, exception, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Warn(this ILoggable loggable, string message,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.Msg(LogEventLevel.Warning, message, exception, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Warn(this ILoggable loggable, TemplatedMessage msgData,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable._Internal_DoMsg(LogEventLevel.Warning, msgData, exception, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Debug(this ILoggable loggable, string message,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.Msg(LogEventLevel.Debug, message, exception, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Debug(this ILoggable loggable, TemplatedMessage msgData,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable._Internal_DoMsg(LogEventLevel.Debug, msgData, exception, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Info(this ILoggable loggable, string message,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.Msg(LogEventLevel.Information, message, exception, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Info(this ILoggable loggable, TemplatedMessage msgData,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable._Internal_DoMsg(LogEventLevel.Information, msgData, exception, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Information(this ILoggable loggable, string message,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.Msg(LogEventLevel.Information, message, exception, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Information(this ILoggable loggable, TemplatedMessage msgData,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable._Internal_DoMsg(LogEventLevel.Information, msgData, exception, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Verbose(this ILoggable loggable, string message,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.Msg(LogEventLevel.Verbose, message, exception, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Verbose(this ILoggable loggable, TemplatedMessage msgData,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable._Internal_DoMsg(LogEventLevel.Verbose, msgData, exception, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool AssertTrueOrLog(this ILoggable loggable, LogEventLevel level, string message,
            bool expression,
            [CallerMemberName] string callerMemberName = null)
        {
            if (!expression)
                loggable.Msg(level, message, null, callerMemberName);

            return expression;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool AssertTrueOrFatal(this ILoggable loggable, string message,
            bool expression,
            [CallerMemberName] string callerMemberName = null)
            => loggable.AssertTrueOrLog(LogEventLevel.Fatal, message, expression, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool AssertTrueOrError(this ILoggable loggable, string message,
            bool expression,
            [CallerMemberName] string callerMemberName = null)
            => loggable.AssertTrueOrLog(LogEventLevel.Error, message, expression, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool AssertTrueOrWarn(this ILoggable loggable, string message,
            bool expression,
            [CallerMemberName] string callerMemberName = null)
            => loggable.AssertTrueOrLog(LogEventLevel.Warning, message, expression, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool AssertTrueOrDebug(this ILoggable loggable, string message,
            bool expression,
            [CallerMemberName] string callerMemberName = null)
            => loggable.AssertTrueOrLog(LogEventLevel.Debug, message, expression, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool AssertTrueOrInfo(this ILoggable loggable, string message,
            bool expression,
            [CallerMemberName] string callerMemberName = null)
            => loggable.AssertTrueOrLog(LogEventLevel.Information, message, expression, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool AssertTrueOrVerbose(this ILoggable loggable, string message,
            bool expression,
            [CallerMemberName] string callerMemberName = null)
            => loggable.AssertTrueOrLog(LogEventLevel.Verbose, message, expression, callerMemberName);

        private static RuntimeFileMetadata _Internal_CreateOrGetMetadata(ILoggable loggable)
        {

            RuntimeFileMetadata derivedMeta = loggable._Internal_GetMetadata();

            // Do we need to do a dictionary lookup?
            if (derivedMeta == null)
            {
                // The derived class does not implement a backing field to store our metadata;
                // OR we do have a backing field but haven't set up our metadata.

                // We need to perform a dictionary lookup.
                bool existing = RuntimeFileMetadata.GetMetadataFromName(loggable.GetType().Name,
                    out RuntimeFileMetadata metadata, cacheIfNecessary: true);

                if (!existing)
                {
                    // Set the metadata since it was just created
                    loggable._Internal_SetMetadata(metadata);

                    if (loggable is INodeDescendant pnLoggable)
                    {
                        // If we're some kind of subcomponent of an instanced node,
                        // let's use log level of the node.
                        metadata.LogLevel = pnLoggable.Node.GetLogLevel();
                    }
                }

                return metadata;
            }

            // No need to perform a dictionary lookup. We have this cached.
            return derivedMeta;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool _Internal_DoMsg(this ILoggable loggable, LogEventLevel level, TemplatedMessage msgData,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null)
        {
            string filename = _Internal_CreateOrGetMetadata(loggable).Filename;
            string memberName = ResolveMemberName(callerMemberName);
            string output = $"[{filename}#{memberName}()] {msgData.Message}";

            if (!loggable.GetLogLevel().IsOn(level))
                return false;

            // This message will be displayed.
            if (msgData.Objects.Length > 0)
            {
                // This is a templated message
                if (exception != null)
                    Serilog.Log.Logger.Write(level, exception, output, propertyValues: msgData.Objects);
                else
                    Serilog.Log.Logger.Write(level, output, propertyValues: msgData.Objects);
            }
            else
            {
                if (exception != null)
                    Serilog.Log.Logger.Write(level, exception, output);
                else
                    Serilog.Log.Logger.Write(level, output);
            }

            // If we're a descendant of a node, let's let the node know about
            // this exception.
            if (exception != null && loggable is INodeDescendant pnLoggable)
                pnLoggable.Node.HandleException(exception);
            
            return true;
        }

    }

}
