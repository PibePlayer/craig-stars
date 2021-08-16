
using System;
using Godot;

namespace CraigStars
{
    public class GDLog : CSLog
    {
        Type type;
        public GDLog(Type type)
        {
            this.type = type;
        }
        public void Debug(object message) => GD.Print("[" + type.Name + "]" + message);
        public void Info(object message) => GD.Print("[" + type.Name + "]" + message);
        public void Error(object message) => GD.PrintErr("[" + type.Name + "]" + message);
        public void Error(object message, Exception e)
        {
            GD.PrintErr("[" + type.Name + "]" + message + "\n" + e.ToString());
            GD.PrintStack();
        }
        public void Warn(object message) => GD.Print("[" + type.Name + "]" + message);
    }
}