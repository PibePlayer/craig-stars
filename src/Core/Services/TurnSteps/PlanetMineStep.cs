using Godot;

namespace CraigStars
{
    /// <summary>
    /// Mine planets for resources
    /// </summary>
    public class PlanetMineStep : Step
    {
        public PlanetMineStep(Game game, TurnGeneratorState state) : base(game, state) { }

        public override void Process()
        {
            OwnedPlanets.ForEach(p =>
            {
                p.Cargo += p.MineralOutput;
                p.MineYears += p.Mines;
                int mineralDecayFactor = Game.Rules.MineralDecayFactor;
                int minMineralConcentration = p.Homeworld ? Game.Rules.MinHomeworldMineralConcentration : Game.Rules.MinMineralConcentration;
                ReduceMineralConcentration(p, mineralDecayFactor, minMineralConcentration);
            });
        }

        /// <summary>
        /// Reduce the mineral concentrations of a planet after mining.
        /// </summary>
        /// <param name="planet">The planet to reduce mineral concentrations for</param>
        /// <param name="mineralDecayFactor">The factor of decay</param>
        /// <param name="minMineralConcentration"></param>
        internal void ReduceMineralConcentration(Planet planet, int mineralDecayFactor, int minMineralConcentration)
        {
            int[] planetMineYears = planet.MineYears;
            int[] planetMineralConcentration = planet.MineralConcentration;
            for (int i = 0; i < 3; i++)
            {
                int conc = planet.MineralConcentration[i];
                int minesPer = mineralDecayFactor / conc / conc;
                int mineYears = planet.MineYears[i];
                if (mineYears > minesPer)
                {
                    conc -= mineYears / minesPer;
                    if (conc < minMineralConcentration)
                    {
                        conc = minMineralConcentration;
                    }
                    mineYears %= minesPer;

                    planetMineYears[i] = mineYears;
                    planetMineralConcentration[i] = conc;
                }
            }
            planet.MineYears = planetMineYears;
            planet.MineralConcentration = planetMineralConcentration;
        }

    }
}