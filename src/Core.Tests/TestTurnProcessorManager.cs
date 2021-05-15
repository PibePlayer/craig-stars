using System.Collections.Generic;

namespace CraigStars.Tests
{
    /// <summary>
    /// A test instance of a TurnProcessorManager that does nothing
    /// </summary>
    public class TestTurnProcessorManager : ITurnProcessorManager
    {
        public IEnumerable<TurnProcessor> TurnProcessors => new List<TurnProcessor>();

        public TurnProcessor GetTurnProcessor(string name)
        {
            return null;
        }

        public void RegisterTurnProcessor<T>() where T : TurnProcessor, new()
        {
        }
    }
}
