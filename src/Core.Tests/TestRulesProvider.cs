
using System.Collections.Generic;

namespace CraigStars.Tests
{
    public class TestRulesProvider : IRulesProvider
    {
        public Rules Rules => new Rules(0);
    }

}