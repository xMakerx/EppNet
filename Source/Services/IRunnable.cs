///////////////////////////////////////////////////////
/// Filename: IRunnable.cs
/// Date: July 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Services
{

    public interface IRunnable
    {
        /// <summary>
        /// Tries to run this runnable
        /// </summary>
        /// <returns>Whether or not the start was successful</returns>
        bool Start();

        /// <summary>
        /// Tries to stop this runnable
        /// </summary>
        /// <returns>Whether or not the stop was successful</returns>
        bool Stop();
    }

}