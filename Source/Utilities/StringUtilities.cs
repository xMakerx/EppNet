///////////////////////////////////////////////////////
/// Filename: StringUtilities.cs
/// Date: September 29, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System.IO;

namespace EppNet.Utilities
{

    public static class StringUtilities
    {

        /// <summary>
        /// Platform agnostic way to get the index of the last path separator
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static int GetLastPathSeparatorIndex(string path)
        {
            int sepIndex;
            int attempts = 0;
            char[] separators = { '/', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

            do
            {
                sepIndex = path.LastIndexOf(separators[attempts++]);
            } while (sepIndex == -1 && attempts < separators.Length);

            return sepIndex;
        }

    }

}
