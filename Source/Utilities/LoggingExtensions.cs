///////////////////////////////////////////////////////
/// Filename: LoggingExtensions.cs
/// Date: September 21, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using Serilog;
using Serilog.Events;

namespace EppNet.Utilities
{
    public static class LoggingExtensions
    {

        /// <summary>
        /// Logs a message at the specified <see cref="LogEventLevel"/> if the specified boolean expression is
        /// true. Returns the passed expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="level"></param>
        /// <param name="msg"></param>
        /// <returns></returns>

        public static bool AssertFalseOrLog(bool expression, LogEventLevel level, string msg)
        {
            if (expression)
                Log.Write(level, msg);

            return expression;
        }

        /// <summary>
        /// Logs a message at the specified <see cref="LogEventLevel"/> if the specified boolean expression is
        /// false. Returns the passed expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="level"></param>
        /// <param name="msg"></param>
        /// <returns></returns>

        public static bool AssertTrueOrLog(bool expression, LogEventLevel level, string msg)
        {
            if (!expression)
                Log.Write(level, msg);

            return expression;
        }

        public static bool AssertTrueOrWarn(bool expression, string msg) => AssertTrueOrLog(expression, LogEventLevel.Warning, msg);
        public static bool AssertTrueOrDebug(bool expression, string msg) => AssertTrueOrLog(expression, LogEventLevel.Debug, msg);
        
        public static bool AssertTrueOrInfo(bool expression, string msg) => AssertTrueOrLog(expression, LogEventLevel.Information, msg);
        public static bool AssertTrueOrFatal(bool expression, string msg) => AssertTrueOrLog(expression, LogEventLevel.Fatal, msg);

    }
}
