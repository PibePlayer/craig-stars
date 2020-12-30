using System;

namespace CraigStars
{
    public class Message
    {
        public MessageType Type { get; set; }
        public String Text { get; set; }
        public MapObject Target { get; set; }

    }
}