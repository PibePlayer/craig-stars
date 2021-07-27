
namespace CraigStars
{
    /// <summary>
    /// Packets decay if sent too fast
    /// </summary>
    public class DecayPacketsStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(DecayPacketsStep));

        public DecayPacketsStep(Game game) : base(game, TurnGenerationState.DecayMineralPackets) { }

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
            var decayRate = 1f - packet.Player.GetPacketDecayRate(packet) * (packet.DistanceTravelled / (packet.WarpFactor * packet.WarpFactor));
            packet.Cargo *= decayRate;
        }

    }
}