using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using Godot;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// Process Packet movement actions
    /// Note: this will be called twice, once at the beginning of a turn, and once after planets grow (for packets that were just launched)
    ///
    /// </summary>
    public class PacketMoveStep : TurnGenerationStep
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PacketMoveStep));
        public const string ProcessedPacketsContextKey = "ProcessedPackets";

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
                packet.Heading = (packet.Target.Position - packet.Position).Normalized();

                packet.Position += packet.Heading * dist;
            }
        }

        internal void CompleteMove(MineralPacket packet)
        {
            // deposit minerals onto the planet
            var cargo = packet.Cargo;
            packet.AttemptTransfer(-cargo);
            packet.Target.AttemptTransfer(cargo);

            // delete the packet
            EventManager.PublishMapObjectDeletedEvent(packet);

            // TODO: handle damage/scan destination
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