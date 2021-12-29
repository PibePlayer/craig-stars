using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// Abstract class for shared functionality between transport tasks
    /// </summary>
    public abstract class AbstractFleetTransportStep : AbstractFleetWaypointProcessStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetUnloadStep));

        List<PlanetInvasion> invasions = new List<PlanetInvasion>();

        private readonly CargoTransferer cargoTransferer;
        private readonly InvasionProcessor invasionProcessor;

        public AbstractFleetTransportStep(
            IProvider<Game> gameProvider,
            IRulesProvider rulesProvider,
            CargoTransferer cargoTransferer,
            InvasionProcessor invasionProcessor,
            TurnGenerationState state) : base(gameProvider, rulesProvider, state, WaypointTask.Transport)
        {
            this.cargoTransferer = cargoTransferer;
            this.invasionProcessor = invasionProcessor;
        }

        /// <summary>
        /// Override PreProcess() to get processed waypoints to the TurnGeneratorContext
        /// </summary>
        public override void PreProcess(List<Planet> ownedPlanets)
        {
            base.PreProcess(ownedPlanets);

            invasions.Clear();
        }

        public override void Process()
        {
            base.Process();

            // process any invasions we may have had
            invasions.ForEach(task => ProcessInvasionTask(task));
        }

        /// <summary>
        /// Transfer minerals or fuel to/from the fleet to a cargoDestination (planet, fleet, deep space, etc)
        /// This will create PlanetInvasions if we beam our colonists to an enemy planet
        /// </summary>
        /// <param name="source">The fleet transferring cargo</param>
        /// <param name="dest">The source or destination giving or receiving the cargo</param>
        /// <param name="cargoType">The type of cargo being transferred</param>
        /// <param name="transferAmount">The amount of cargo being transferred</param>
        protected void Transfer(Fleet source, ICargoHolder dest, CargoType cargoType, int transferAmount)
        {
            if (cargoType == CargoType.Fuel)
            {
                cargoTransferer.Transfer(source, dest, Cargo.Empty, transferAmount);
            }
            else if (cargoType == CargoType.Colonists)
            {
                // invasion?
                if (dest is Planet planet && planet.PlayerNum != source.PlayerNum)
                {
                    if (transferAmount > 0)
                    {
                        invasions.Add(new PlanetInvasion()
                        {
                            Planet = planet,
                            Fleet = source,
                            ColonistsToDrop = transferAmount * 100
                        });
                        // remove colonists from our cargo
                        source.Cargo = source.Cargo - Cargo.OfAmount(cargoType, transferAmount);
                    }
                    else
                    {
                        // can't beam enemy colonists onto your ship...
                        // TODO: send a message
                        log.Warn($"{Game.Year}: {source.PlayerNum} {source.Name} tried to beam colonists up from: {dest}");
                    }
                }
                else if (dest is Fleet otherFleet && otherFleet.PlayerNum != source.PlayerNum)
                {
                    // ignore this, but send a message
                    // TODO: send a message
                    log.Warn($"{Game.Year}: {source.PlayerNum} {source.Name} tried to transfer colonists to/from a fleet they don't own: {otherFleet}");
                }
                else
                {
                    cargoTransferer.Transfer(source, dest, Cargo.OfAmount(cargoType, transferAmount), 0);

                    log.Debug($"{Game.Year}: {source.PlayerNum} {source.Name} transferred {transferAmount}kT of {cargoType} to {dest.Name}");
                }
            }
            else
            {
                cargoTransferer.Transfer(source, dest, Cargo.OfAmount(cargoType, transferAmount), 0);
                log.Debug($"{Game.Year}: {source.PlayerNum} {source.Name} transferred {transferAmount}kT of {cargoType} to {dest.Name}");
            }
        }

        /// <summary>
        /// After we build a list of planet invasions, run them
        /// </summary>
        void ProcessInvasionTask(PlanetInvasion task)
        {
            invasionProcessor.InvadePlanet(task.Planet, Game.Players[task.Planet.PlayerNum], Game.Players[task.Fleet.PlayerNum], task.Fleet, task.ColonistsToDrop);
        }

    }
}