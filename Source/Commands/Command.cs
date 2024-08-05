///////////////////////////////////////////////////////
/// Filename: Command.cs
/// Date: August 4, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;
using EppNet.Node;
using EppNet.Time;
using EppNet.Utilities;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EppNet.Commands
{

    public static class Commands
    {

        internal static readonly List<SlottableEnum> _cmdsList = new List<SlottableEnum>();

        public static readonly SlottableEnum None = SlottableEnum._Internal_CreateAndAddTo(_cmdsList, "None", 1);
    }

    public interface ICommandTarget { }

}
