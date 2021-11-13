
namespace CraigStars
{
    /// <summary>
    /// Lay mines
    /// </summary>
    public class DecayMinesStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(DecayMinesStep));

        public DecayMinesStep(IProvider<Game> gameProvider) : base(gameProvider, TurnGenerationState.MineLaying) { }

        public override void Process()
        {

            foreach (var mineField in Game.MineFields)
            {
                Decay(mineField);
            }
        }

        /// <summary>
        /// Decay minefields
        /// </summary>
        /// <param name="mineField"></param>
        internal void Decay(MineField mineField)
        {
            long decayedMines = mineField.GetDecayRate(Game.Players[mineField.PlayerNum], Game.Planets, Game.Rules);
            mineField.NumMines -= decayedMines;

            // 10 mines or less, minefield goes away
            if (mineField.NumMines <= 10)
            {
                EventManager.PublishMapObjectDeletedEvent(mineField);
            }
        }


    }
}