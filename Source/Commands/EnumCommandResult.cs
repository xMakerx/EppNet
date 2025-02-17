//////////////////////////////////////////////
/// Filename: EnumCommandResult.cs
/// Date: August 4, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

namespace EppNet.Commands
{

    public enum EnumCommandResult
    {

        /// <summary>
        /// The command executed successfully.
        /// </summary>
        Ok                  = 0,

        /// <summary>
        /// The relevant objects could not be found and the command
        /// could not proceed.
        /// </summary>
        NotFound            = 1,

        /// <summary>
        /// The relevant objects aren't in the correct state to
        /// proceed with the command.
        /// </summary>
        InvalidState        = 2,

        /// <summary>
        /// Provided argument(s) aren't valid for the specified command.
        /// </summary>
        BadArgument         = 3,

        /// <summary>
        /// The relevant service to execute the command is offline or inaccessible.
        /// </summary>
        NoService           = 4,

        /// <summary>
        /// The command is unavailable.
        /// </summary>
        Unavailable         = 5,

        /// <summary>
        /// Nodes of relevant objects don't match<br/>
        /// Cannot mismatch objects of different nodes due to sandboxing
        /// </summary>
        NodeMismatch        = 6
    }

    public static class EnumCommandResultExtensions
    {

        public static bool IsOk(this EnumCommandResult result)
            => result == EnumCommandResult.Ok;
        public static bool IsOk(EnumCommandResult? result)
            => result.HasValue &&
            result.Value.IsOk();

    }

}