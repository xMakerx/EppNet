//////////////////////////////////////////////
/// Filename: LogService.cs
/// Date: July 10, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Services;

using Serilog;

namespace EppNet.Logging
{

    /// <summary>
    /// This service is essentially a Serilog wrapper.
    /// </summary>

    public class LogService : Service
    {

        protected LoggerConfiguration _loggerConfig;

        public LogService(ServiceManager svcMgr) : base(svcMgr)
        {
            this._loggerConfig = new();
        }

        internal override void Update(float dt)
        {
            if (Status == ServiceState.Starting)
            {
                // Create the logger
                Log.Logger = _loggerConfig.CreateLogger();
                Status = ServiceState.Online;
            }
        }

    }

}