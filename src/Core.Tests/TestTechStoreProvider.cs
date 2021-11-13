
using System.Collections.Generic;

namespace CraigStars.Tests
{
    public class TestTechStoreProvider : IProvider<ITechStore>
    {
        public ITechStore Item => StaticTechStore.Instance;
    }

}