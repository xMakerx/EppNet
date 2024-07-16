/////////////////////////////////////////////
/// Filename: EnumObjectState.cs
/// Date: September 27, 2023
/// Author: Maverick Liberty
//////////////////////////////////////////////

namespace EppNet.Objects
{

    public enum EnumObjectState
    {

        /// <summary>
        /// State is unknown or -- more likely -- doesn't exist.
        /// </summary>
        Unknown             = 0,

        /// <summary>
        /// Object is awaiting the most up-to-date state so it can generate
        /// </summary>
        WaitingForState     = 1,

        /// <summary>
        /// Object is currently generating
        /// </summary>
        Generating          = 2,

        /// <summary>
        /// Object has been generated
        /// </summary>
        Generated           = 3,

        /// <summary>
        /// Object is disabled and isn't sending new updates
        /// </summary>
        Disabled            = 4,

        /// <summary>
        /// Object is pending delete
        /// </summary>
        PendingDelete       = 5,

        /// <summary>
        /// Object existed previously but has since been deleted.
        /// </summary>
        Deleted             = 6
    }

}
