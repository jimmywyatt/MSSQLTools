using log4net.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MSSQLTools.Helpers
{
    public static class LogHelper
    {
        private static log4net.ILog _log;

        public static log4net.ILog Log4Net
        {
            get
            {
                if (_log == null)
                {
                    _log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
                    XmlConfigurator.Configure();
                }

                return _log;
            }
        }
    }
}
