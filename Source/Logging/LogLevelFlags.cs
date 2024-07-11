//////////////////////////////////////////////
/// Filename: LogLevelFlags.cs
/// Date: July 10, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Utilities;

using Serilog.Events;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EppNet.Logging
{

    [Flags]
    public enum LogLevelFlags
    {

        None                = 0,
        Verbose             = 1 << 0,
        Debug               = 1 << 1,
        Info                = 1 << 2,
        Warn                = 1 << 3,
        Error               = 1 << 4,
        Fatal               = 1 << 5,

        InfoErrorFatal      = Info | Error | Fatal,
        InfoWarnFatal       = Info | Warn | Fatal,

        /// <summary>
        /// The default log levels
        /// </summary>
        InfoWarnErrorFatal  = Info | Warn | Error | Fatal,

        All                 = ~(~0 << 6)
    }

    public static class LogLevelFlagsExtensions
    {

        private static readonly ReadOnlyDictionary<LogEventLevel, LogLevelFlags> _level2Flag = new Dictionary<LogEventLevel, LogLevelFlags>
        {
            { LogEventLevel.Verbose, LogLevelFlags.Verbose   },
            { LogEventLevel.Debug,   LogLevelFlags.Debug     },
            { LogEventLevel.Information, LogLevelFlags.Info  },
            { LogEventLevel.Warning, LogLevelFlags.Warn      },
            { LogEventLevel.Error, LogLevelFlags.Error       },
            { LogEventLevel.Fatal, LogLevelFlags.Fatal       }
        }.AsReadOnly();

        private static readonly ReadOnlyDictionary<LogLevelFlags, LogEventLevel> _flag2Level = new Dictionary<LogLevelFlags, LogEventLevel>
        {
            { LogLevelFlags.Verbose, LogEventLevel.Verbose   },
            { LogLevelFlags.Debug , LogEventLevel.Debug     },
            { LogLevelFlags.Info, LogEventLevel.Information  },
            { LogLevelFlags.Warn , LogEventLevel.Warning      },
            { LogLevelFlags.Error , LogEventLevel.Error       },
            { LogLevelFlags.Fatal , LogEventLevel.Fatal       }
        }.AsReadOnly();

        /// <summary>
        /// Converts a Serilog <see cref="LogEventLevel"/> to our <see cref="LogLevelFlags"/>
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static LogLevelFlags ToFlag(this LogEventLevel level) => _level2Flag[level];

        /// <summary>
        /// Converts a <see cref="LogLevelFlags"/> flag to a Serilog <see cref="LogEventLevel"/><br/>
        /// NOTE: <see cref="LogLevelFlags"/> MUST have only 1 bit enabled or this will return NULL.
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static LogEventLevel? ToLevel(this LogLevelFlags flags)
        {
            // Checks to see if only one flag is set
            if ((flags & (flags - 1)) != 0)
                return _flag2Level[flags];
            else
                return null;
        }

        public static bool IsOn(this LogLevelFlags flags, LogEventLevel level) => flags.IsFlagSet(level.ToFlag());

        public static bool IsOn(this LogLevelFlags flags, LogLevelFlags level) => flags.IsFlagSet(level);

    }

}