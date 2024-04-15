///////////////////////////////////////////////////////
/// Filename: Configuration.cs
/// Date: April 15, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Core.Settings
{

    internal class Configuration
    {

        public ConfigurationGroup MainGroup { get => _mainGroup; }

        protected ConfigurationGroup _mainGroup = new(null);

        public void Write()
        {



        }

    }
}
