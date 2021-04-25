using System;

namespace CraigStars
{
    public interface CSLog
    {
        void Debug(object message);
        void Info(object message);
        void Error(object message);
        void Error(object message, Exception e);
        void Warn(object message);
    }
}