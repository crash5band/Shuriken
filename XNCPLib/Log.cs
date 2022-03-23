using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNCPLib
{
    public static class Log
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void Info(string message, string loggerName = "Logger")
        {
            logger.Log(new LogEventInfo(LogLevel.Info, loggerName, message));
        }
        public static void Warn(string message, string loggerName = "Logger")
        {
            logger.Log(new LogEventInfo(LogLevel.Warn, loggerName, message));
        }
        public static void Exception(string message, string loggerName = "Logger")
        {
            logger.Log(new LogEventInfo(LogLevel.Error, loggerName, message));
        }

    }
}
