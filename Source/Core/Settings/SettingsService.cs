///////////////////////////////////////////////////////
/// Filename: SettingsService.cs
/// Date: April 14, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

using EppNet.Services;

namespace EppNet.Core.Settings
{

    public class SettingsService : Service
    {

        /// <summary>
        /// The path to the directory where the configuration is stored.
        /// </summary>

        public string ConfigurationPath
        {
            set
            {
                MarkDirty();
                _configPath = value;
            }

            get => _configPath;
        }

        /// <summary>
        /// The filename and extension of the configuration
        /// </summary>

        public string ConfigurationFilename
        {
            set
            {
                _configFilename = value;
            }

            get => _configFilename;
        }
        public string GetFullFilePath() => @$"{ConfigurationPath}\{ConfigurationFilename}";

        private string _configPath = AppDomain.CurrentDomain.BaseDirectory + @"\config";
        private string _configFilename = "config.json";

        public SettingsService(ServiceManager svcMgr) : base(svcMgr)
        {

        }


    }

}
