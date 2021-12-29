using System.Collections.Generic;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Move Fleets in space
    /// </summary>
    public class FleetBattleStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetBattleStep));

        private readonly BattleEngine battleEngine;
        private readonly FleetSpecService fleetSpecService;

        public FleetBattleStep(IProvider<Game> gameProvider, BattleEngine battleEngine, FleetSpecService fleetSpecService) : base(gameProvider, TurnGenerationState.FleetBattleStep)
        {
            this.battleEngine = battleEngine;
            this.fleetSpecService = fleetSpecService;
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
            Dictionary<Vector2, List<MapObject>> mapObjectsByLocation = new(Game.MapObjectsByLocation);
            foreach (var entry in mapObjectsByLocation)
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
                                if (fleet is Starbase starbase)
                                {
                                    var player = Game.Players[starbase.PlayerNum];
                                    if (player.Race.Spec.LivesOnStarbases)
                                    {
                                        // kill this AR planet.
                                        var planet = starbase.Orbiting;
                                        planet.Population = 0;
                                        planet.Mines = player.Race.Spec.InnateMining ? 0 : planet.Mines;
                                        planet.Factories = player.Race.Spec.InnateResources ? 0 : planet.Factories;
                                        EventManager.PublishPlanetPopulationEmptiedEvent(planet);
                                    }
                                }

                                // this fleet was destroyed
                                EventManager.PublishMapObjectDeletedEvent(fleet);
                            }
                            else
                            {
                                // update specs after battle
                                fleetSpecService.ComputeFleetSpec(Game.Players[fleet.PlayerNum], fleet, recompute: true);
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