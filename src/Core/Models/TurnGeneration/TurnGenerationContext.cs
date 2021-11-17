using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// Context about a turn is passed along each step in this context variable
    /// </summary>
    public class TurnGenerationContext
    {
        public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();
    }
}