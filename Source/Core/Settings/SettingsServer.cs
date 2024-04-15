///////////////////////////////////////////////////////
/// Filename: SettingsServer.cs
/// Date: April 14, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Core.Settings
{

    public class SettingsServer
    {

        public static string ConfigurationPath
        {
            set
            {
                _configPath = value;
                // TODO: Ask for new config
            }

            get => _configPath;
        }

        protected static string _configPath = AppDomain.CurrentDomain.BaseDirectory + @"\config";

        public static string GetFullFilePath() => @$"{_configPath}\config.json";

    }

}
