
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Packets decay if sent too fast
    /// </summary>
    public class DecayPacketsStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(DecayPacketsStep));

        private readonly IRulesProvider rulesProvider;
        private Rules Rules => rulesProvider.Rules;

        public DecayPacketsStep(IProvider<Game> gameProvider, IRulesProvider rulesProvider) : base(gameProvider, TurnGenerationState.DecayMineralPackets)
        {
            this.rulesProvider = rulesProvider;
        }

        public override void Process()
        {
            foreach (var packet in Game.MineralPackets)
            {
                Decay(packet);
            }
        }

        /// <summary>
        /// https://wiki.starsautohost.org/wiki/%22Mass_Packet_FAQ%22_by_Barry_Kearns_1997-02-07_v2.6b
        /// Depending on how fast a packet is thrown compared to it's safe speed, it decays
        /// </summary>
        /// <param name="packet"></param>
        internal void Decay(MineralPacket packet)
        {
            var player = Game.Players[packet.PlayerNum];
            var decayRate = 1f - GetPacketDecayRate(player.Race, packet) * (packet.DistanceTravelled / (packet.WarpFactor * packet.WarpFactor));
            packet.Cargo *= decayRate;
        }

        /// <summary>
        /// PP races can fling packets 1 warp faster without decaying.
        /// </summary>
        public float GetPacketDecayRate(Race race, MineralPacket packet)
        {
            int overSafeWarp = packet.WarpFactor - packet.SafeWarpSpeed;

            // IT is always count as being at least 1 over the safe warp
            overSafeWarp += race.Spec.PacketOverSafeWarpPenalty;

            // we only care about packets thrown up to 3 warp over the limit 
            overSafeWarp = Mathf.Clamp(packet.WarpFactor - packet.SafeWarpSpeed, 0, 3);

            var packetDecayRate = 0f;
            if (overSafeWarp > 0)
            {
                packetDecayRate = Rules.PacketDecayRate[overSafeWarp];
            }

            // PP have half the decay rate
            packetDecayRate *= race.Spec.PacketDecayFactor;

            return packetDecayRate;
        }

    }

}