using System.Linq;
using CraigStars.Utils;
using Godot;
using static CraigStars.Utils.Utils;

namespace CraigStars
{
    /// <summary>
    /// Lay mines
    /// </summary>
    public class FleetLayMinesStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetLayMinesStep));

        public FleetLayMinesStep(IProvider<Game> gameProvider) : base(gameProvider, TurnGenerationState.FleetLayMinesStep) { }

        public override void Process()
        {

            // Separate our waypoint tasks into groups
            foreach (var fleet in Game.Fleets.Where(fleet =>
                fleet.Waypoints[0].Task == WaypointTask.LayMineField))
            {
                var player = Game.Players[fleet.PlayerNum];
                if (!fleet.Spec.CanLayMines)
                {
                    fleet.Waypoints[0].Task = WaypointTask.None;
                    Message.MinesLaidFailed(player, fleet);
                }
                else
                {
                    LayMineField(fleet, player);
                }
            }
        }

        internal void LayMineField(Fleet fleet, Player player)
        {
            foreach (var entry in fleet.Spec.MineLayingRateByMineType)
            {
                int minesLaid = entry.Value;
                if (fleet.Waypoints.Count > 1)
                {
                    minesLaid = (int)(minesLaid * player.Race.Spec.MineFieldRateMoveFactor);
                }

                // we aren't laying mines (probably because we're moving, skip it)
                if (minesLaid == 0)
                {
                    continue;
                }

                // see if we are adding to an existing minefield
                var mineField = LocateExistingMineField(player, fleet.Position, entry.Key);
                if (mineField == null)
                {
                    mineField = new MineField()
                    {
                        Name = $"{player.Race.Name} {EnumUtils.GetLabelForMineFieldType(entry.Key)} Mine Field",
                        PlayerNum = player.Num,
                        Position = fleet.Position,
                        Type = entry.Key
                    };
                    EventManager.PublishMapObjectCreatedEvent(mineField);
                }

                // add to it!
                long currentMines = mineField.NumMines;
                mineField.NumMines += minesLaid;
                Message.MinesLaid(player, fleet, mineField, minesLaid);

                if (mineField.Position != fleet.Position)
                {
                    // move this minefield closer to us (in case it's not in our location)
                    // This was taken from the FreeStars codebase (like many other things)
                    mineField.Position = new Vector2(
                        minesLaid / mineField.NumMines * (fleet.Position.x - mineField.Position.x) + mineField.Position.x,
                        minesLaid / mineField.NumMines * (fleet.Position.y - mineField.Position.y) + mineField.Position.y
                    );
                }
            }
        }

        /// <summary>
        /// Find an existing minefield that contains this position and is owned by the player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="position"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        MineField LocateExistingMineField(Player player, Vector2 position, MineFieldType type)
        {
            foreach (var mineField in Game.MineFields.Where(mf => mf.PlayerNum == player.Num && mf.Type == type))
            {
                if (IsPointInCircle(position, mineField.Position, mineField.Radius))
                {
                    return mineField;
                }
            }
            return null;
        }

    }
}
