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
    public class FleetBattleStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetBattleStep));

        BattleEngine battleEngine;

        public FleetBattleStep(Game game) : base(game, TurnGenerationState.Battle)
        {
            battleEngine = new BattleEngine(Game.Rules);
        }

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
                    battleEngine.FindMoveTargets(battle);

                    if (battle.HasTargets)
                    {
                        // run the battle!
                        battleEngine.RunBattle(battle);
                        foreach (var fleet in fleetsAndStarbases)
                        {
                            var hasTokens = false;
                            foreach (var token in fleet.Tokens)
                            {
                                if (token.Quantity > 0)
                                {
                                    hasTokens = true;
                                    break;
                                }
                            }

                            if (!hasTokens)
                            {
                                // this fleet was destroyed
                                EventManager.PublishMapObjectDeletedEvent(fleet);
                            }
                            else
                            {
                                // update aggregates after battle
                                fleet.ComputeAggregate(true);
                            }
                        }

                        foreach (var playerEntry in battle.PlayerRecords)
                        {
                            // let each player know about this battle
                            playerEntry.Key.Battles.Add(playerEntry.Value);
                        }

                        EventManager.PublishBattleRunEvent(battle);
                    }
                }
            }
        }



    }
}