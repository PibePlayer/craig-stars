using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// Service for player's discovering map objects and updating their intel
    /// </summary>
    public class PlayerIntel
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PlayerIntel));

        PlanetDiscoverer planetDiscoverer = new PlanetDiscoverer();
        FleetDiscoverer fleetDiscoverer = new FleetDiscoverer();
        ShipDesignDiscoverer designDiscoverer = new ShipDesignDiscoverer();

        /// <summary>
        /// Discover a new planet. This is called when the universe is being setup with penScanned = false
        /// so we generate a bunch of empty planet reports.
        /// When we later scan a planet, it'll be called with penScanned = true
        /// </summary>
        /// <param name="player"></param>
        /// <param name="planet"></param>
        /// <param name="penScanned"></param>
        public void Discover(Player player, Planet planet, bool penScanned = false)
        {
            planetDiscoverer.Discover(player, planet, penScanned);
        }

        /// <summary>
        /// Players are only aware of fleets they are actively scanning. We remove all fleet report
        /// data at the beginning of a turn
        /// </summary>
        /// <param name="player"></param>
        public void ClearFleetReports(Player player)
        {
            player.Fleets.Clear();
            player.ForeignFleets.Clear();
            player.FleetsByGuid.Clear();
            foreach (var planet in player.AllPlanets)
            {
                planet.OrbitingFleets.Clear();
            }
        }

        /// <summary>
        /// Discover a new planet. This is called when the universe is being setup with penScanned = false
        /// so we generate a bunch of empty planet reports.
        /// When we later scan a planet, it'll be called with penScanned = true
        /// </summary>
        /// <param name="player"></param>
        /// <param name="planet"></param>
        /// <param name="penScanned"></param>
        public void Discover(Player player, Fleet fleet, bool penScanned = false)
        {
            fleetDiscoverer.Discover(player, fleet, penScanned);
        }

        /// <summary>
        /// Discover a new planet. This is called when the universe is being setup with penScanned = false
        /// so we generate a bunch of empty planet reports.
        /// When we later scan a planet, it'll be called with penScanned = true
        /// </summary>
        /// <param name="player"></param>
        /// <param name="planet"></param>
        /// <param name="penScanned"></param>
        public void Discover(Player player, ShipDesign design, bool penScanned = false)
        {
            designDiscoverer.Discover(player, design, penScanned);
        }
    }
}
