﻿///////////////////////////////////////////////////////
/// Filename: LoggingExtensions.cs
/// Date: September 21, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Logging;

using Serilog;
using Serilog.Events;

using System.Runtime.CompilerServices;

namespace EppNet.Utilities
{

    public struct CallerInformation
    {

        public string MemberName;
        public string Filepath;
        public CallerInformation(string memberName, string filepath)
        {
            this.MemberName = memberName;
            this.Filepath = filepath;
        }

    }

    public static class LoggingExtensions
    {

        public static LogEventLevel GlobalLogLevel;

        public static string GetRuntimePath([CallerFilePath] string callerFilepath = null) => callerFilepath;

        public static string Msg(LogEventLevel level, string message, [CallerFilePath] string callerFilepath = null, [CallerMemberName] string callerMemberName = null)
        {
            string filename = RuntimeFileMetadata.GetFilenameFromPath(callerFilepath, cacheIfNecessary: true);
            string memberName = callerMemberName == ".ctor" ? "ctor" : callerMemberName;
            string output = $"[{filename}#{memberName}()] {message}";

            Log.Write(level, output);
            return output;
        }
        
        public static void Info(string message, [CallerFilePath] string callerFilepath = null, 
            [CallerMemberName] string callerMemberName = null) => Msg(LogEventLevel.Information, message, callerFilepath, callerMemberName);

        public static void Info(string message, out string fullMessage, [CallerFilePath] string callerFilepath = null,
            [CallerMemberName] string callerMemberName = null)
        {
            fullMessage = Msg(LogEventLevel.Information, message, callerFilepath, callerMemberName);
        }

        public static void Warn(string message, [CallerFilePath] string callerFilepath = null,
            [CallerMemberName] string callerMemberName = null) => Msg(LogEventLevel.Warning, message, callerFilepath, callerMemberName);

        public static void Warn(string message, out string fullMessage, [CallerFilePath] string callerFilepath = null,
            [CallerMemberName] string callerMemberName = null)
        {
            fullMessage = Msg(LogEventLevel.Warning, message, callerFilepath, callerMemberName);
        }

        public static void Debug(string message, [CallerFilePath] string callerFilepath = null,
            [CallerMemberName] string callerMemberName = null) => Msg(LogEventLevel.Debug, message, callerFilepath, callerMemberName);

        public static void Debug(string message, out string fullMessage, [CallerFilePath] string callerFilepath = null,
            [CallerMemberName] string callerMemberName = null)
        {
            fullMessage = Msg(LogEventLevel.Debug, message, callerFilepath, callerMemberName);
        }

        public static void Verbose(string message, [CallerFilePath] string callerFilepath = null,
            [CallerMemberName] string callerMemberName = null) => Msg(LogEventLevel.Verbose, message, callerFilepath, callerMemberName);

        public static void Verbose(string message, out string fullMessage, [CallerFilePath] string callerFilepath = null,
            [CallerMemberName] string callerMemberName = null)
        {
            fullMessage = Msg(LogEventLevel.Verbose, message, callerFilepath, callerMemberName);
        }

        public static void Error(string message, [CallerFilePath] string callerFilepath = null,
            [CallerMemberName] string callerMemberName = null) => Msg(LogEventLevel.Error, message, callerFilepath, callerMemberName);

        public static void Error(string message, out string fullMessage, [CallerFilePath] string callerFilepath = null,
            [CallerMemberName] string callerMemberName = null)
        {
            fullMessage = Msg(LogEventLevel.Error, message, callerFilepath, callerMemberName);
        }

        public static void Fatal(string message, [CallerFilePath] string callerFilepath = null,
            [CallerMemberName] string callerMemberName = null) => Msg(LogEventLevel.Fatal, message, callerFilepath, callerMemberName);

        public static void Fatal(string message, out string fullMessage, [CallerFilePath] string callerFilepath = null,
            [CallerMemberName] string callerMemberName = null)
        {
            fullMessage = Msg(LogEventLevel.Fatal, message, callerFilepath, callerMemberName);
        }

        /// <summary>
        /// Logs a message at the specified <see cref="LogEventLevel"/> if the specified boolean expression is
        /// true. Returns the passed expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <returns></returns>

        public static bool AssertFalseOrLog(bool expression, LogEventLevel level, string message, [CallerFilePath] string callerFilepath = null,
            [CallerMemberName] string callerMemberName = null)
        {
            if (expression)
                Msg(level, message, callerFilepath, callerMemberName);

            return expression;
        }

        /// <summary>
        /// Logs a message at the specified <see cref="LogEventLevel"/> if the specified boolean expression is
        /// false. Returns the passed expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <returns></returns>

        public static bool AssertTrueOrLog(bool expression, LogEventLevel level, string message, [CallerFilePath] string callerFilepath = null,
            [CallerMemberName] string callerMemberName = null)
        {
            if (!expression)
                Msg(level, message, callerFilepath, callerMemberName);

            return expression;
        }

        public static bool AssertTrueOrInfo(bool expression, string message, [CallerFilePath] string callerFilepath = null,
            [CallerMemberName] string callerMemberName = null)
            => AssertTrueOrLog(expression, LogEventLevel.Information, message, callerFilepath, callerMemberName);

        public static bool AssertTrueOrWarn(bool expression, string message, [CallerFilePath] string callerFilepath = null,
            [CallerMemberName] string callerMemberName = null)
            => AssertTrueOrLog(expression, LogEventLevel.Warning, message, callerFilepath, callerMemberName);

        public static bool AssertTrueOrDebug(bool expression, string message, [CallerFilePath] string callerFilepath = null,
            [CallerMemberName] string callerMemberName = null)
            => AssertTrueOrLog(expression, LogEventLevel.Debug, message, callerFilepath, callerMemberName);

        public static bool AssertTrueOrVerbose(bool expression, string message, [CallerFilePath] string callerFilepath = null,
            [CallerMemberName] string callerMemberName = null)
            => AssertTrueOrLog(expression, LogEventLevel.Verbose, message, callerFilepath, callerMemberName);

        public static bool AssertTrueOrError(bool expression, string message, [CallerFilePath] string callerFilepath = null,
            [CallerMemberName] string callerMemberName = null)
            => AssertTrueOrLog(expression, LogEventLevel.Error, message, callerFilepath, callerMemberName);

        public static bool AssertTrueOrFatal(bool expression, string message, [CallerFilePath] string callerFilepath = null,
            [CallerMemberName] string callerMemberName = null)
            => AssertTrueOrLog(expression, LogEventLevel.Fatal, message, callerFilepath, callerMemberName);

    }
}
