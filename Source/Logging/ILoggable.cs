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
        protected internal void _Internal_SetMetadata(RuntimeFileMetadata metadata);

        protected internal RuntimeFileMetadata _Internal_GetMetadata();
    }

    public static class ILoggableExtensions
    {

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void _Internal_SetupLogging(this ILoggable loggable, [CallerFilePath] string callerFilepath = null)
        {
            loggable._Internal_SetMetadata(RuntimeFileMetadata.GetMetadataFromPath(
                callerFilepath, cacheIfNecessary: true));
        }

        /// <summary>
        /// Sets the <see cref="LogEventLevel"/>
        /// </summary>
        /// <param name="loggable"></param>
        /// <param name="level"></param>

        public static void SetLogLevel(this ILoggable loggable, LogEventLevel level)
        {
            RuntimeFileMetadata metadata = loggable._Internal_GetMetadata();

            if (metadata == null)
                // We've got BIGGG problems
                return;

            metadata.LogLevel = level;
        }

        /// <summary>
        /// Gets the <see cref="LogEventLevel"/> for this loggable.<br/>
        /// If no <see cref="LogEventLevel"/> was set, the following takes precendence:<br/>
        /// - If Loggable is <see cref="IPerNode"/>, uses the <see cref="LogEventLevel"/> of the associated <see cref="NetworkNode"/><br/>
        /// - If not a <see cref="IPerNode"/>, uses <see cref="LoggingExtensions.GlobalLogLevel"/>
        /// </summary>
        /// <param name="loggable"></param>
        /// <returns></returns>

        public static LogEventLevel GetLogLevel(this ILoggable loggable)
        {
            RuntimeFileMetadata metadata = loggable._Internal_GetMetadata();
            LogEventLevel? runtimeLevel = metadata?.LogLevel;

            LogEventLevel result;

            if (runtimeLevel.HasValue)
                result = runtimeLevel.Value;
            else if (loggable is IPerNode pnLoggable)
                result = pnLoggable.Node.GetLogLevel();
            else
                result = LoggingExtensions.GlobalLogLevel;

            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Msg(this ILoggable loggable, LogEventLevel level, string message,
            Exception exception = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null)
        {
            
            // We need to ensure that we have runtime metadata along with
            // log level information available.
            RuntimeFileMetadata metadata = loggable._Internal_GetMetadata();
            if (metadata == null)
            {
                // Metadata not available. Let's set it up.
                loggable._Internal_SetupLogging(callerFilePath);
                metadata = loggable._Internal_GetMetadata();
            }

            string filename = metadata.Filename;
            string memberName = callerMemberName == ".ctor" ? "ctor" : callerMemberName;
            string output = $"[{filename}#{memberName}()] {message}";

            LogEventLevel myLevel = loggable.GetLogLevel();
            Serilog.Log.Write(level, exception, output);

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Fatal(this ILoggable loggable, string message,
            Exception exception = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null) 
            => loggable.Msg(LogEventLevel.Fatal, message, exception, callerFilePath, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Error(this ILoggable loggable, string message,
            Exception exception = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.Msg(LogEventLevel.Error, message, exception, callerFilePath, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Warning(this ILoggable loggable, string message,
            Exception exception = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.Msg(LogEventLevel.Warning, message, exception, callerFilePath, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Warn(this ILoggable loggable, string message,
            Exception exception = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.Msg(LogEventLevel.Warning, message, exception, callerFilePath, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Debug(this ILoggable loggable, string message,
            Exception exception = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.Msg(LogEventLevel.Debug, message, exception, callerFilePath, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Info(this ILoggable loggable, string message,
            Exception exception = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.Msg(LogEventLevel.Information, message, exception, callerFilePath, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Information(this ILoggable loggable, string message,
            Exception exception = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.Msg(LogEventLevel.Information, message, exception, callerFilePath, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Verbose(this ILoggable loggable, string message,
            Exception exception = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.Msg(LogEventLevel.Verbose, message, exception, callerFilePath, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool AssertTrueOrLog(this ILoggable loggable, LogEventLevel level, string message,
            bool expression,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null)
        {
            if (!expression)
                loggable.Msg(level, message, null, callerFilePath, callerMemberName);

            return expression;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool AssertTrueOrFatal(this ILoggable loggable, string message,
            bool expression,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.AssertTrueOrLog(LogEventLevel.Fatal, message, expression, callerFilePath, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool AssertTrueOrError(this ILoggable loggable, string message,
            bool expression,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.AssertTrueOrLog(LogEventLevel.Error, message, expression, callerFilePath, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool AssertTrueOrWarn(this ILoggable loggable, string message,
            bool expression,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.AssertTrueOrLog(LogEventLevel.Warning, message, expression, callerFilePath, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool AssertTrueOrDebug(this ILoggable loggable, string message,
            bool expression,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.AssertTrueOrLog(LogEventLevel.Debug, message, expression, callerFilePath, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool AssertTrueOrInfo(this ILoggable loggable, string message,
            bool expression,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.AssertTrueOrLog(LogEventLevel.Information, message, expression, callerFilePath, callerMemberName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool AssertTrueOrVerbose(this ILoggable loggable, string message,
            bool expression,
            [CallerFilePath] string callerFilePath = null,
            [CallerMemberName] string callerMemberName = null)
            => loggable.AssertTrueOrLog(LogEventLevel.Verbose, message, expression, callerFilePath, callerMemberName);

    }

}
