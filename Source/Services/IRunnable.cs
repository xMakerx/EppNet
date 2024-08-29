///////////////////////////////////////////////////////
/// Filename: IRunnable.cs
/// Date: July 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Services
{

    public interface IRunnable
    {

        public bool Started { get; }

        /// <summary>
        /// Tries to run this runnable
        /// </summary>
        /// <returns>Whether or not the start was successful</returns>
        bool Start();

        /// <summary>
        /// Tries to tick this runnable<br/>
        /// Requires <see cref="Started"/> to be true
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        bool Tick(float dt)
        {
            if (!Started)
                return false;

            return Started;
        }

        /// <summary>
        /// Tries to stop this runnable
        /// </summary>
        /// <returns>Whether or not the stop was successful</returns>
        bool Stop();
    }

}