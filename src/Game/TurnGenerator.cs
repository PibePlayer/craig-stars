using System;
using System.Collections.Generic;
using System.Linq;

public class TurnGenerator
{
    /// <summary>
    /// Generate a turn
    /// 
    /// Stars! Order of Events
    /// <c>
    ///     Scrapping fleets (w/possible tech gain) 
    ///     Waypoint 0 unload tasks 
    ///     Waypoint 0 Colonization/Ground Combat resolution (w/possible tech gain) 
    ///     Waypoint 0 load tasks 
    ///     Other Waypoint 0 tasks * 
    ///     MT moves 
    ///     In-space packets move and decay 
    ///     PP packets (de)terraform 
    ///     Packets cause damage 
    ///     Wormhole entry points jiggle 
    ///     Fleets move (run out of fuel, hit minefields (fields reduce as they are hit), stargate, wormhole travel) 
    ///     Inner Strength colonists grow in fleets 
    ///     Mass Packets still in space and Salvage decay 
    ///     Wormhole exit points jiggle 
    ///     Wormhole endpoints degrade/jump 
    ///     SD Minefields detonate (possibly damaging again fleet that hit minefield during movement) 
    ///     Mining 
    ///     Production (incl. research, packet launch, fleet/starbase construction) 
    ///     SS Spy bonus obtained 
    ///     Population grows/dies 
    ///     Packets that just launched and reach their destination cause damage 
    ///     Random events (comet strikes, etc.) 
    ///     Fleet battles (w/possible tech gain) 
    ///     Meet MT 
    ///     Bombing 
    ///     Waypoint 1 unload tasks 
    ///     Waypoint 1 Colonization/Ground Combat resolution (w/possible tech gain) 
    ///     Waypoint 1 load tasks 
    ///     Mine Laying 
    ///     Fleet Transfer 
    ///     CA Instaforming 
    ///     Mine sweeping 
    ///     Starbase and fleet repair 
    ///     Remote Terraforming
    /// </c>
    /// </summary>
    /// <param name="game"></param>
    public void GenerateTurn(Game game)
    {
        game.Year++;

        var ownedPlanets = game.Universe.Planets.Where(p => p.Player != null).ToList();
        Mine(game.UniverseSettings, ownedPlanets);
        Produce(game.Universe.Planets);
        Grow(game.Universe.Planets);
    }

    void Mine(UniverseSettings settings, List<Planet> planets)
    {
        planets.ForEach(p =>
        {
            p.Cargo.Add(p.GetMineralOutput());
            p.MineYears.Add(p.Mines);
            // TODO: figure out settings
            int mineralDecayFactor = settings.MineralDecayFactor;
            int minMineralConcentration = p.Homeworld ? settings.MinHomeworldMineralConcentration : settings.MinMineralConcentration;
            ReduceMineralConcentration(p, mineralDecayFactor, minMineralConcentration);
        });
    }

    /// <summary>
    /// Reduce the mineral concentrations of a planet after mining.
    /// </summary>
    /// <param name="planet">The planet to reduce mineral concentrations for</param>
    /// <param name="mineralDecayFactor">The factor of decay</param>
    /// <param name="minMineralConcentration"></param>
    void ReduceMineralConcentration(Planet planet, int mineralDecayFactor, int minMineralConcentration)
    {
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

                planet.MineYears[i] = mineYears;
                planet.MineralConcentration[i] = conc;
            }
        }
    }

    void Produce(List<Planet> planets)
    {
        // TODO: build stuff
    }

    /// <summary>
    /// Grow populations on planets
    /// </summary>
    void Grow(List<Planet> planets)
    {
        planets.ForEach(p => p.Population += p.GetGrowthAmount());
    }
}