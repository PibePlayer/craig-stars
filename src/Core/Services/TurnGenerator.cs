using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using log4net;

namespace CraigStars
{
    public class TurnGenerator
    {
        static CSLog log = LogProvider.GetLogger(typeof(TurnGenerator));

        public event Action<TurnGenerationState> TurnGeneratorAdvancedEvent;
        public void PublishTurnGeneratorAdvancedEvent(TurnGenerationState state) => TurnGeneratorAdvancedEvent?.Invoke(state);

        Game Game { get; }

        // the steps executed by the TurnGenerator
        List<TurnGenerationStep> steps;

        /// <summary>
        /// Stars! Order of Events
        /// <c>
        ///     Scrapping fleets (w/possible tech gain) 
        ///     Waypoint 0 unload tasks 
        ///     Waypoint 0 Colonization/Ground Combat resolution (w/possible tech gain) 
        ///     Waypoint 0 load tasks 
        ///     Other Waypoint 0 tasks * 
        ///     MT moves 
        ///     In-space packets move and decay 
        ///     PP packets (de)terraform 
        ///     Packets cause damage 
        ///     Fleets move (run out of fuel, hit minefields (fields reduce as they are hit), stargate, wormhole travel) 
        ///     Inner Strength colonists grow in fleets 
        ///     Mass Packets still in space and Salvage decay 
        ///     Wormholes Jiggle
        ///     SD Minefields detonate (possibly damaging again fleet that hit minefield during movement) 
        ///     Mining 
        ///     Production (incl. research, packet launch, fleet/starbase construction) 
        ///     SS Spy bonus obtained 
        ///     Population grows/dies 
        ///     Packets that just launched and reach their destination cause damage 
        ///     Random events (comet strikes, etc.) 
        ///     Fleet battles (w/possible tech gain) 
        ///     Meet MT 
        ///     Bombing 
        ///     Waypoint 1 unload tasks 
        ///     Waypoint 1 Colonization/Ground Combat resolution (w/possible tech gain) 
        ///     Waypoint 1 load tasks 
        ///     Mine Decay 
        ///     Mine Laying 
        ///     Fleet Transfer 
        ///     CA Instaforming 
        ///     Mine sweeping 
        ///     Starbase and fleet repair 
        ///     Remote Terraforming
        ///     Scan and Update Player Intel
        /// </c>
        /// </summary>
        /// <param name="game"></param>
        public TurnGenerator(Game game)
        {
            Game = game;
            steps = new List<TurnGenerationStep>() {
                new FleetWaypointStep(game, 0), // wp0
                new PacketMoveStep(game, 0),
                new FleetMoveStep(game),
                new DecaySalvageStep(game),
                new DecayPacketsStep(game),
                new WormholeJiggleStep(game),
                new DetonateMinesStep(game),
                new PlanetMineStep(game),
                new PlanetProductionStep(game),
                new PlayerResearchStep(game),
                new PlanetGrowStep(game),
                new PacketMoveStep(game, 1),
                new FleetBattleStep(game),
                new PlanetBombStep(game),
                new FleetWaypointStep(game, 1), // wp1
                new DecayMinesStep(game),
                new FleetLayMinesStep(game),
                new FleetSweepMinesStep(game),
                new PlayerScanStep(game),
                new CalculateScoreStep(game),
            };
        }


        /// <summary>
        /// Generate a turn
        /// </summary>
        public void GenerateTurn()
        {
            PublishTurnGeneratorAdvancedEvent(TurnGenerationState.Scrapping);
            Game.Year++;

            // reset the players for a new turn
            log.Debug($"{Game.Year}: Resetting players");
            Game.Players.ForEach(p =>
            {
                p.SubmittedTurn = false;
                p.Messages.Clear();
                p.ComputeAggregates();
            });

            // execute each turn step
            var context = new TurnGenerationContext();
            var ownedPlanets = Game.OwnedPlanets.ToList();
            foreach (var step in steps)
            {
                Game.PurgeDeletedMapObjects();
                PublishTurnGeneratorAdvancedEvent(step.State);
                step.Execute(context, ownedPlanets);
            }

        }
    }
}