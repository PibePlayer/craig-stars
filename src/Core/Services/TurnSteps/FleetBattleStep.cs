using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using Godot;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// Move Fleets in space
    /// </summary>
    public class FleetBattleStep : Step
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FleetBattleStep));

        BattleEngine battleEngine = new BattleEngine();

        public FleetBattleStep(Game game, TurnGeneratorState state) : base(game, state) { }

        public override void PreProcess(List<Planet> ownedPlanets)
        {
            base.PreProcess(ownedPlanets);

            // figure out who is all bunched together
            Game.UpdateMapObjectsByLocation();
        }

        public override void Process()
        {
            // this list of lists of fleets are all the fleets that are in combat
            var fleetsInCombat = new List<List<Fleet>>();

            foreach (var entry in Game.MapObjectsByLocation)
            {
                var fleetsAndStarbases = new List<Fleet>();
                var players = new HashSet<Player>();
                foreach (var mapObject in entry.Value)
                {
                    if (mapObject is Planet planet && planet.Starbase != null)
                    {
                        fleetsAndStarbases.Add(planet.Starbase);
                        players.Add(planet.Player);
                    }
                    else if (mapObject is Fleet fleet)
                    {
                        fleetsAndStarbases.Add(fleet);
                        players.Add(fleet.Player);
                    }
                }

                // we have more than one player! getting close!
                if (players.Count > 1)
                {
                    // build a new battle engine and find targets
                    var battle = battleEngine.BuildBattle(fleetsAndStarbases);
                    battleEngine.FindTargets(battle);

                    if (battle.HasTargets)
                    {
                        var fleet = fleetsAndStarbases[0];
                        if (fleet.Orbiting != null)
                        {
                            log.Info($"Running a battle at {fleet.Orbiting.Name} involving {players.Count} players and {fleetsAndStarbases.Count} fleets.");
                        }
                        else
                        {
                            log.Info($"Running a battle at {fleet.Position} involving {players.Count} players and {fleetsAndStarbases.Count} fleets.");
                        }

                        // run the battle!
                        battleEngine.RunBattle(battle);
                        EventManager.PublishBattleRunEvent(battle);
                    }
                }
            }
        }



    }
}