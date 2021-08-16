using System;
using log4net;

namespace CraigStars
{
    public class Log4NetLog : CSLog
    {
        ILog log;
        public Log4NetLog(Type type)
        {
            log = LogManager.GetLogger(type);
        }
        public void Debug(object message) => log.Debug(message);
        public void Info(object message) => log.Info(message);
        public void Error(object message) => log.Error(message);
        public void Error(object message, Exception e) => log.Error(message, e);
        public void Warn(object message) => log.Warn(message);
    }
}