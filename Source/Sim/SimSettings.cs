///////////////////////////////////////////////////////
/// Filename: SimSettings.cs
/// Date: October 10, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
using System;

namespace EppNet.Sim
{

    public static class SimSettings
    {

        private static float _updates_per_second = 60f;

        public static float UpdatesPerSecond
        {
            set
            {

                if (value == _updates_per_second)
                    return;

                if (value < 1f)
                {
                    Serilog.Log.Fatal("[SimSettings.UpdatesPerSecond] Invalid update rate! Must be at least 1 per second!");
                    return;
                }

                if (value > 100f)
                {
                    Serilog.Log.Fatal("[SimSettings.UpdatesPerSecond] Invalid update rate! Must be at least 1 but less than 100 per second.");
                    return;
                }

                // Was provided a valid update rate.
                _updates_per_second = value;

                // Acknowledge the update
                UpdateRateChanged?.Invoke();
            }

            get => _updates_per_second;
        }

        public static Action UpdateRateChanged;

        /// <summary>
        /// Calculate the milliseconds between updates.
        /// </summary>
        /// <returns></returns>

        public static int CalculateMsBetweenUpdates() => (int)Math.Ceiling(1000f / _updates_per_second);

    }

}
