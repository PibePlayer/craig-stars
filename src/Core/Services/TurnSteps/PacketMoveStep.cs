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
    public class PacketMoveStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(PacketMoveStep));
        public const string ProcessedPacketsContextKey = "ProcessedPackets";

        PlanetService planetService = new();

        HashSet<MineralPacket> processedMineralPackets = new HashSet<MineralPacket>();

        // some things (like remote mining) only happen on wp1
        int processIndex = 0;

        public PacketMoveStep(Game game, int waypointIndex) : base(game, TurnGenerationState.Waypoint)
        {
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
                MovePacket(packet);
            }
        }

        private void MovePacket(MineralPacket packet)
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
                CompleteMove(packet);
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
        internal void CompleteMove(MineralPacket packet)
        {
            // deposit minerals onto the planet
            var cargo = packet.Cargo;
            var weight = packet.Cargo.Total;

            var planet = packet.Target;

            if (packet.Target.HasMassDriver && planet.Starbase.Aggregate.SafePacketSpeed >= packet.WarpFactor)
            {
                // caught packet successfully, transfer cargo
                packet.Target.AttemptTransfer(cargo);
                Message.MineralPacketCaught(packet.Target.Player, packet.Target, packet);
            }
            else if (planet.Player == null)
            {
                packet.Target.AttemptTransfer(cargo);
            }
            else if (planet.Player != null)
            {
                // uh oh, this packet is going to fast and we'll take damage
                var receiverDriverSpeed = planet.HasStarbase ? planet.Starbase.Aggregate.SafePacketSpeed : 0;

                var speedOfPacket = packet.WarpFactor * packet.WarpFactor;
                var speedOfReceiver = receiverDriverSpeed * receiverDriverSpeed;
                var percentCaughtSafely = (float)speedOfReceiver / speedOfPacket;
                var mineralsRecovered = weight * percentCaughtSafely + weight * (1 / 3f) * (1 - percentCaughtSafely);
                var rawDamage = (speedOfPacket - speedOfReceiver) * weight / 160f;
                var damageWithDefenses = rawDamage * (1 - planetService.GetDefenseCoverage(planet, planet.Player));
                var colonistsKilled = RoundToNearest(Math.Max(damageWithDefenses * planet.Population / 1000f, damageWithDefenses * 100));
                var defensesDestroyed = (int)Math.Max(planet.Defenses * damageWithDefenses / 1000, damageWithDefenses / 20);

                // kill off colonists and defenses
                planet.Population = RoundToNearest(Mathf.Clamp(planet.Population - colonistsKilled, 0, planet.Population));
                planet.Defenses = Mathf.Clamp(planet.Defenses - (int)defensesDestroyed, 0, planet.Defenses);

                Message.MineralPacketDamage(planet.Player, planet, packet, colonistsKilled, defensesDestroyed);
                if (planet.Population == 0)
                {
                    EventManager.PublishPlanetPopulationEmptiedEvent(planet);
                }
            }

            // if we didn't recieve this planet, notify the sender
            if (planet.Player != packet.Player)
            {
                Message.MineralPacketArrived(packet.Player, planet, packet);
            }

            // delete the packet
            EventManager.PublishMapObjectDeletedEvent(packet);
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