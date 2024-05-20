///////////////////////////////////////////////////////
/// Filename: SettingsService.cs
/// Date: April 14, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Core.Settings
{

    public class SettingsService : Service
    {

        /// <summary>
        /// The path to the directory where the configuration is stored.
        /// </summary>

        public static string ConfigurationPath
        {
            set
            {
                _configPath = value;
                // TODO: Ask for new config
            }

            get => _configPath;
        }

        /// <summary>
        /// The filename and extension of the configuration
        /// </summary>

        public static string ConfigurationFilename
        {
            set
            {
                _configFilename = value;
            }

            get => _configFilename;
        }
        public static string GetFullFilePath() => @$"{ConfigurationPath}\{ConfigurationFilename}";

        private static string _configPath = AppDomain.CurrentDomain.BaseDirectory + @"\config";
        private static string _configFilename = "config.json";


    }

}
