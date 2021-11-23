using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static CraigStars.Utils.Utils;

namespace CraigStars
{
    /// <summary>
    /// Process Packet movement actions
    /// Note: this will be called twice, once at the beginning of a turn, and once after planets grow (for packets that were just launched)
    ///
    /// </summary>
    public abstract class AbstractPacketMoveStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(AbstractPacketMoveStep));
        public const string ProcessedPacketsContextKey = "ProcessedPackets";

        private readonly PlanetService planetService;
        private readonly ShipDesignDiscoverer designDiscoverer;
        private readonly IRulesProvider rulesProvider;
        private Rules Rules => rulesProvider.Rules;

        HashSet<MineralPacket> processedMineralPackets = new HashSet<MineralPacket>();

        // some things (like remote mining) only happen on wp1
        private readonly int processIndex;

        public AbstractPacketMoveStep(IProvider<Game> gameProvider, PlanetService planetService, ShipDesignDiscoverer designDiscoverer, IRulesProvider rulesProvider, TurnGenerationState state, int waypointIndex) : base(gameProvider, state)
        {
            this.planetService = planetService;
            this.designDiscoverer = designDiscoverer;
            this.rulesProvider = rulesProvider;
            this.processIndex = waypointIndex;
        }

        /// <summary>
        /// Override PreProcess() to get processed waypoints to the TurnGeneratorContext
        /// </summary>
        public override void PreProcess(List<Planet> ownedPlanets)
        {
            base.PreProcess(ownedPlanets);

            processedMineralPackets.Clear();

            if (Context.Context.TryGetValue(ProcessedPacketsContextKey, out var packets)
            && packets is HashSet<MineralPacket> processedWaypointsFromContext)
            {
                // add any processed waypoints from the context
                processedMineralPackets.UnionWith(processedWaypointsFromContext);
            }
        }

        public override void Process()
        {
            // process any packets we haven't processed yet.
            foreach (var packet in Game.MineralPackets.Where(p => !processedMineralPackets.Contains(p)))
            {
                MovePacket(packet, Game.Players[packet.PlayerNum]);
            }
        }

        void MovePacket(MineralPacket packet, Player player)
        {
            float dist = packet.WarpFactor * packet.WarpFactor;
            float totalDist = packet.Position.DistanceTo(packet.Target.Position);

            // remote mining happens in wp1, before transport tasks
            if (processIndex == 1)
            {
                // half move packets...
                dist /= 2;
            }

            // go with the lower
            if (totalDist < dist)
            {
                dist = totalDist;
            }

            if (totalDist == dist)
            {
                CompleteMove(packet, player);
            }
            else
            {
                // move this fleet closer to the next waypoint
                packet.WarpFactor = packet.WarpFactor;
                packet.DistanceTravelled = dist;
                packet.Heading = (packet.Target.Position - packet.Position).Normalized();

                packet.Position += packet.Heading * dist;
            }
        }

        /// <summary>
        /// Damage calcs the Stars! Manual
        /// 
        /// Example:
        /// You fling a 1000kT packet at Warp 10 at a planet with a Warp 5 driver, a population of 250,000 and 50 defenses preventing 60% of incoming damage.
        /// spdPacket = 100
        /// spdReceiver = 25
        /// %CaughtSafely = 25%
        /// minerals recovered = 1000kT x 25% + 1000kT x 75% x 1/3 = 250 + 250 = 500kT 
        /// dmgRaw = 75 x 1000 / 160 = 469
        /// dmgRaw2 = 469 x 40% = 188
        /// #colonists killed = Max. of ( 188 x 250,000 / 1000, 188 x 100)
        /// = Max. of ( 47,000, 18800) = 47,000 colonists
        /// #defenses destroyed = 50 * 188 / 1000 = 9 (rounded down)
        /// 
        /// If, however, the receiving planet had no mass driver or defenses, the damage is far greater:
        /// minerals recovered = 1000kT x 0% + 1000kT x 100% x 1/3 = only 333kT dmgRaw = 100 x 1000 / 160 = 625
        /// dmgRaw2 = 625 x 100% = 625
        /// #colonists killed = Max. of (625 x 250,000 / 1000, 625 x 100)
        /// = Max.of(156,250, 62500) = 156,250.
        /// If the packet increased speed up to Warp 13, then:
        /// dmgRaw2 = dmgRaw = 169 x 1000 / 160 = 1056
        /// #colonists killed = Max. of (1056 x 250,000 / 1000, 1056 x 100)
        /// = Max.of( 264,000, 105600) destroying the colony
        /// </summary>
        /// <param name="packet"></param>
        internal void CompleteMove(MineralPacket packet, Player player)
        {
            // deposit minerals onto the planet
            var cargo = packet.Cargo;
            var weight = packet.Cargo.Total;

            var planet = packet.Target;

            int uncaught = 0;

            if (packet.Target.HasMassDriver && planet.Starbase.Spec.SafePacketSpeed >= packet.WarpFactor)
            {
                // caught packet successfully, transfer cargo
                packet.Target.AttemptTransfer(cargo);
                Message.MineralPacketCaught(Game.Players[packet.Target.PlayerNum], packet.Target, packet);
            }
            else if (!planet.Owned)
            {
                packet.Target.AttemptTransfer(cargo);

                // all uncaught
                uncaught = weight;
            }
            else if (planet.Owned)
            {
                var planetPlayer = Game.Players[planet.PlayerNum];
                // uh oh, this packet is going to fast and we'll take damage
                var receiverDriverSpeed = planet.HasStarbase ? planet.Starbase.Spec.SafePacketSpeed : 0;

                var speedOfPacket = packet.WarpFactor * packet.WarpFactor;
                var speedOfReceiver = receiverDriverSpeed * receiverDriverSpeed;
                var percentCaughtSafely = (float)speedOfReceiver / speedOfPacket;
                uncaught = (int)((1f - percentCaughtSafely) * weight);
                var mineralsRecovered = weight * percentCaughtSafely + weight * (1 / 3f) * (1 - percentCaughtSafely);
                var rawDamage = (speedOfPacket - speedOfReceiver) * weight / 160f;
                var damageWithDefenses = rawDamage * (1 - planetService.GetDefenseCoverage(planet, planetPlayer));
                var colonistsKilled = RoundToNearest(Math.Max(damageWithDefenses * planet.Population / 1000f, damageWithDefenses * 100));
                var defensesDestroyed = (int)Math.Max(planet.Defenses * damageWithDefenses / 1000, damageWithDefenses / 20);

                // kill off colonists and defenses
                planet.Population = RoundToNearest(Mathf.Clamp(planet.Population - colonistsKilled, 0, planet.Population));
                planet.Defenses = Mathf.Clamp(planet.Defenses - (int)defensesDestroyed, 0, planet.Defenses);

                Message.MineralPacketDamage(planetPlayer, planet, packet, colonistsKilled, defensesDestroyed);
                if (planet.Population == 0)
                {
                    EventManager.PublishPlanetPopulationEmptiedEvent(planet);
                }
            }

            // if we didn't recieve this planet, notify the sender
            if (planet.PlayerNum != packet.PlayerNum)
            {
                if (player.Race.Spec.DetectPacketDestinationStarbases && planet.HasStarbase)
                {
                    // discover the receiving planet's starbase design
                    designDiscoverer.Discover(player, planet.Starbase.Design, true);
                }

                Message.MineralPacketArrived(player, planet, packet);
            }

            if (uncaught > 0)
            {
                CheckTerraform(packet, player, planet, uncaught);
                CheckPermaform(packet, player, planet, uncaught);
            }

            // delete the packet
            EventManager.PublishMapObjectDeletedEvent(packet);
        }

        /// <summary>
        /// From mazda on starsautohost: https://starsautohost.org/sahforum2/index.php?t=msg&th=1294&start=0&rid=0
        /// 
        /// For each uncaught 100kT of mineral there is 50% chance of performing 1 click of normal terraforming on the target planet (i.e. the same terra with the same limits as if you were sat on the planet spending resources).
        /// So a large packet of 2000kT on an unoccupied planet should expect to terrafrom it 10 clicks, which is a lot.
        /// 
        /// Secondly, the same packet can also perform "permanent terraforming" by altering the underlying planet variables.
        /// There is a 50% chance of performing 1 click of permanent terraforming per 1000kT of uncaught material.
        /// 
        /// This ability is only available to PP and CA and for CAs it is random, whereas PP's can choose what planet to permanently alter.
        /// 
        /// Note these figures are for uncaught amounts, so work best on planets with no defences and especially no flingers.
        /// 
        /// The direction of terraforming in all cases is towards the optimum for the flinging race. 
        /// Whether the target is yours, friends, enemies or empty makes no difference.
        /// 
        /// For an immunity I believe the terraforming is towards the closest edge.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="player"></param>
        /// <param name="planet"></param>
        /// <param name="uncaught"></param>
        internal void CheckTerraform(MineralPacket packet, Player player, Planet planet, int uncaught)
        {
            if (player.Race.Spec.PacketTerraformChance > 0)
            {
                // we have a 50% chance per 100kt of uncaught minerals, so if we have 200kT uncaught, we 
                // check twice
                for (int uncaughtCheck = player.Race.Spec.PacketPermaTerraformSizeUnit; uncaughtCheck <= uncaught; uncaughtCheck += player.Race.Spec.PacketPermaTerraformSizeUnit)
                {
                    if (player.Race.Spec.PacketTerraformChance >= Rules.Random.NextDouble())
                    {
                        // terraform one at a time to ensure the best things get terraformed
                        var result = planetService.TerraformOneStep(planet, player);
                        if (result.Terraformed)
                        {
                            Message.PacketTerraform(player, planet, result.type, result.direction);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Same as above, but the chances are lower
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="player"></param>
        /// <param name="planet"></param>
        /// <param name="uncaught"></param>
        internal void CheckPermaform(MineralPacket packet, Player player, Planet planet, int uncaught)
        {
            if (player.Race.Spec.PacketPermaformChance > 0 && player.Race.Spec.PacketPermaTerraformSizeUnit > 0)
            {
                // we have a 50% chance per 100kt of uncaught minerals, so if we have 200kT uncaught, we 
                // check twice
                for (int uncaughtCheck = player.Race.Spec.PacketPermaTerraformSizeUnit; uncaughtCheck <= uncaught; uncaughtCheck += player.Race.Spec.PacketPermaTerraformSizeUnit)
                {
                    if (player.Race.Spec.PacketPermaformChance >= Rules.Random.NextDouble())
                    {
                        // terraform one at a time to ensure the best things get terraformed
                        HabType habType = (HabType)Rules.Random.Next(3);
                        var result = planetService.PermaformOneStep(planet, player, habType);
                        if (result.Terraformed)
                        {
                            Message.PacketPermaform(player, planet, result.type, result.direction);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Override PostProcess() to set processed waypoints to the TurnGeneratorContext
        /// </summary>
        public override void PostProcess()
        {
            base.PostProcess();
            Context.Context[ProcessedPacketsContextKey] = processedMineralPackets;
        }



    }
}