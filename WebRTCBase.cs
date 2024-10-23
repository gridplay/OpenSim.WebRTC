using log4net;
using Nini.Config;
using OpenSim.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenSim.WebRTC
{
    public class WebRTCBase : ServiceBase
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected bool w_Enabled = false;
        public WebRTCBase(IConfigSource config) : base(config)
        {
            IConfig rtcConfig = config.Configs["WebRTCService"];
            w_Enabled = rtcConfig.GetBoolean("Enabled", false);
        }
    }
}
