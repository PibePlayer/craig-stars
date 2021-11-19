
using System.Collections.Generic;

namespace CraigStars.Tests
{
    public class TestRulesProvider : IRulesProvider
    {
        public Rules TestRules { get; set; } = new Rules(0);
        public Rules Rules => TestRules;
    }

}