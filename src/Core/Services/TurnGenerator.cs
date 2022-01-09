using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    public class TurnGenerator
    {
        static CSLog log = LogProvider.GetLogger(typeof(TurnGenerator));

        public event Action<TurnGenerationState> TurnGeneratorAdvancedEvent;
        public event Action PurgeDeletedMapObjectsEvent;
        public void PublishTurnGeneratorAdvancedEvent(TurnGenerationState state) => TurnGeneratorAdvancedEvent?.Invoke(state);
        public void PublishPurgeDeletedMapObjectsEvent() => PurgeDeletedMapObjectsEvent?.Invoke();

        Game Game { get; }

        // the steps executed by the TurnGenerator
        IList<TurnGenerationStep> steps;

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
        public TurnGenerator(Game game, IList<TurnGenerationStep> steps)
        {
            Game = game;
            this.steps = steps;
        }

        /// <summary>
        /// Generate a turn
        /// </summary>
        public void GenerateTurn()
        {
            PublishTurnGeneratorAdvancedEvent(TurnGenerationState.FleetScrapStep);
            Game.Year++;

            // reset the players for a new turn
            log.Debug($"{Game.Year}: Resetting players");
            Game.Players.ForEach(p =>
            {
                p.Messages.Clear();
                p.Battles.Clear();
                p.BattlesByGuid.Clear();
            });

            // execute each turn step
            var context = new TurnGenerationContext();
            var ownedPlanets = Game.OwnedPlanets.ToList();
            foreach (var step in steps.OrderBy(step => (int)step.State))
            {
                PublishPurgeDeletedMapObjectsEvent();
                PublishTurnGeneratorAdvancedEvent(step.State);
                step.Execute(context, ownedPlanets);
            }

        }
    }
}