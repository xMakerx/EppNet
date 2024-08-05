//////////////////////////////////////////////
/// Filename: EnumCommandResult.cs
/// Date: August 4, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

namespace EppNet.Commands
{

    public enum EnumCommandResult
    {

        Ok                  = 0,
        NotFound            = 1,
        InvalidState        = 2,
        BadArgument         = 3,
        NoService           = 4,
        Unavailable         = 5
    }

    public static class EnumCommandResultExtensions
    {

        public static bool IsOk(this EnumCommandResult result) => result == EnumCommandResult.Ok;
        public static bool IsOk(EnumCommandResult? result) => result.HasValue && result.Value.IsOk();

    }

}