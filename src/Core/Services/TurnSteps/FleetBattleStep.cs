using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// Move Fleets in space
    /// </summary>
    public class FleetBattleStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetBattleStep));

        private readonly BattleEngine battleEngine;
        private readonly FleetAggregator fleetAggregator;

        public FleetBattleStep(IProvider<Game> gameProvider, BattleEngine battleEngine, FleetAggregator fleetAggregator) : base(gameProvider, TurnGenerationState.FleetBattleStep)
        {
            this.battleEngine = battleEngine;
            this.fleetAggregator = fleetAggregator;
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
                var players = new HashSet<int>();
                foreach (var mapObject in entry.Value)
                {
                    if (mapObject is Planet planet && planet.Starbase != null)
                    {
                        fleetsAndStarbases.Add(planet.Starbase);
                        players.Add(planet.PlayerNum);
                    }
                    else if (mapObject is Fleet fleet)
                    {
                        fleetsAndStarbases.Add(fleet);
                        players.Add(fleet.PlayerNum);
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
                                fleetAggregator.ComputeAggregate(Game.Players[fleet.PlayerNum], fleet, recompute: true);
                            }
                        }

                        // records are keyed by player num
                        // for each record, let the involved players know about this record
                        foreach (var recordEntry in battle.PlayerRecords)
                        {
                            var player = Game.Players[recordEntry.Key];
                            // let each player know about this battle
                            player.Battles.Add(recordEntry.Value);
                        }

                        EventManager.PublishBattleRunEvent(battle);
                    }
                }
            }
        }



    }
}