using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// Service for player's discovering map objects and updating their intel
    /// </summary>
    public class PlayerIntel
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlayerIntel));

        private readonly PlanetDiscoverer planetDiscoverer;
        private readonly FleetDiscoverer fleetDiscoverer;
        private readonly ShipDesignDiscoverer designDiscoverer;
        private readonly MineFieldDiscoverer mineFieldDiscoverer;
        private readonly MineralPacketDiscoverer mineralPacketDiscoverer;
        private readonly SalvageDiscoverer salvageDiscoverer;
        private readonly WormholeDiscoverer wormholeDiscoverer;
        private readonly MysteryTraderDiscoverer mysterytraderDiscoverer;

        public PlayerIntel(
            PlanetDiscoverer planetDiscoverer,
            FleetDiscoverer fleetDiscoverer,
            ShipDesignDiscoverer designDiscoverer,
            MineFieldDiscoverer mineFieldDiscoverer,
            MineralPacketDiscoverer mineralPacketDiscoverer,
            SalvageDiscoverer salvageDiscoverer,
            WormholeDiscoverer wormholeDiscoverer,
            MysteryTraderDiscoverer mysterytraderDiscoverer
        )
        {
            this.planetDiscoverer = planetDiscoverer;
            this.fleetDiscoverer = fleetDiscoverer;
            this.designDiscoverer = designDiscoverer;
            this.mineFieldDiscoverer = mineFieldDiscoverer;
            this.mineralPacketDiscoverer = mineralPacketDiscoverer;
            this.salvageDiscoverer = salvageDiscoverer;
            this.wormholeDiscoverer = wormholeDiscoverer;
            this.mysterytraderDiscoverer = mysterytraderDiscoverer;
        }


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
        /// Discover a new fleet. Pen Scanned fleets show tokens and designs
        /// </summary>
        /// <param name="player"></param>
        /// <param name="fleet"></param>
        /// <param name="penScanned"></param>
        public void Discover(Player player, Fleet fleet, bool penScanned = false)
        {
            fleetDiscoverer.Discover(player, fleet, penScanned);
        }

        /// <summary>
        /// Discover a new ShipDesign
        /// </summary>
        /// <param name="player"></param>
        /// <param name="design"></param>
        /// <param name="inBattle">Pen Scanned designs show hull components</param>
        public void Discover(Player player, ShipDesign design, bool inBattle = false)
        {
            designDiscoverer.Discover(player, design, inBattle);
        }

        /// <summary>
        /// Discover a minefield
        /// </summary>
        /// <param name="player"></param>
        /// <param name="mineField"></param>
        /// <param name="penScanned"></param>
        public void Discover(Player player, MineField mineField, bool penScanned = false)
        {
            mineFieldDiscoverer.Discover(player, mineField, penScanned);
        }

        /// <summary>
        /// Discover a mineral pacaket
        /// </summary>
        /// <param name="player"></param>
        /// <param name="mineralPacket"></param>
        /// <param name="penScanned"></param>
        public void Discover(Player player, MineralPacket mineralPacket, bool penScanned = false)
        {
            mineralPacketDiscoverer.Discover(player, mineralPacket, penScanned);
        }

        /// <summary>
        /// Discover salvage
        /// </summary>
        /// <param name="player"></param>
        /// <param name="salvage"></param>
        /// <param name="penScanned"></param>
        public void Discover(Player player, Salvage salvage, bool penScanned = false)
        {
            salvageDiscoverer.Discover(player, salvage, penScanned);
        }

        /// <summary>
        /// Discover a wormhole
        /// </summary>
        /// <param name="player"></param>
        /// <param name="wormhole"></param>
        /// <param name="penScanned"></param>
        public void Discover(Player player, Wormhole wormhole, bool penScanned = false)
        {
            wormholeDiscoverer.Discover(player, wormhole, penScanned);
        }

        /// <summary>
        /// Discover a mysterytrader
        /// </summary>
        /// <param name="player"></param>
        /// <param name="mysterytrader"></param>
        /// <param name="penScanned"></param>
        public void Discover(Player player, MysteryTrader mysterytrader, bool penScanned = false)
        {
            mysterytraderDiscoverer.Discover(player, mysterytrader, penScanned);
        }

        /// <summary>
        /// Players are only aware of fleets/salvage/etc if they are actively scanning. 
        /// We remove all transient report data at the beginning of a turn
        /// </summary>
        /// <param name="player"></param>
        public void ClearTransientReports(Player player)
        {
            player.SalvageIntel.Clear();
            player.FleetIntel.Clear();
            player.MineralPacketIntel.Clear();
            foreach (var planet in player.AllPlanets)
            {
                planet.OrbitingFleets.Clear();
            }
        }

    }
}
