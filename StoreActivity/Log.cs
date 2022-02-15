using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msa.StoreActivity
{
    public class Log
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));
        // private static readonly log4net.Config.XmlConfigurator.Configure();

        public Log()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public void logInfo(string content)
        {
            log.Info(content);
        }

        public void logError(string content)
        {
            log.Error(content);
        }
    }
}
