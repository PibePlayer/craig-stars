
namespace CraigStars
{
    /// <summary>
    /// Lay mines
    /// </summary>
    public class DecayMinesStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(DecayMinesStep));

        private readonly MineFieldDecayer mineFieldDecayer;

        public DecayMinesStep(IProvider<Game> gameProvider, MineFieldDecayer mineFieldDecayer) : base(gameProvider, TurnGenerationState.MineLaying)
        {
            this.mineFieldDecayer = mineFieldDecayer;
        }

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
            long decayedMines = mineFieldDecayer.GetDecayRate(mineField, Game.Players[mineField.PlayerNum], Game.Planets);
            mineField.NumMines -= decayedMines;

            // 10 mines or less, minefield goes away
            if (mineField.NumMines <= 10)
            {
                EventManager.PublishMapObjectDeletedEvent(mineField);
            }
        }


    }
}