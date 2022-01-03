using System;
using System.Collections.Generic;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// This service executes client side fleet orders on the server
    /// </summary>
    public class CargoTransferer
    {
        static CSLog log = LogProvider.GetLogger(typeof(CargoTransferer));

        /// <summary>
        /// If any of our fleets at this location can steal cargo, so can we.
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="mapObjectsByLocation"></param>
        public bool GetCanStealFleetCargo(Fleet fleet, Dictionary<Vector2, List<MapObject>> mapObjectsByLocation)
        {
            bool canStealFleetCargo = fleet.Spec.CanStealFleetCargo;

            // if we can steal cargo, return true, we're done
            // otherwise, see if nay other fleets here can
            if (!canStealFleetCargo && mapObjectsByLocation.TryGetValue(fleet.Position, out var mapObjectsAtLocation))
            {
                canStealFleetCargo = mapObjectsAtLocation.Find(mo => mo is Fleet fleet && fleet.OwnedBy(fleet.PlayerNum) && fleet.Spec.CanStealFleetCargo) != null;
            }

            return canStealFleetCargo;
        }

        /// <summary>
        /// If any of our fleets at this location can steal cargo, so can we.
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="mapObjectsByLocation"></param>
        public bool GetCanStealPlanetCargo(Fleet fleet, Dictionary<Vector2, List<MapObject>> mapObjectsByLocation)
        {
            bool canStealPlanetCargo = fleet.Spec.CanStealPlanetCargo;

            // if we can steal cargo, return true, we're done
            // otherwise, see if nay other fleets here can
            if (!canStealPlanetCargo && mapObjectsByLocation.TryGetValue(fleet.Position, out var mapObjectsAtLocation))
            {
                canStealPlanetCargo = mapObjectsAtLocation.Find(mo => mo is Fleet fleet && fleet.OwnedBy(fleet.PlayerNum) && fleet.Spec.CanStealPlanetCargo) != null;
            }

            return canStealPlanetCargo;
        }

        /// <summary>
        /// True if this source is allowed to transfer cargo from this dest
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public bool CanTransfer(ICargoHolder source, ICargoHolder dest, Dictionary<Vector2, List<MapObject>> mapObjectsByLocation)
        {
            // only SS fleets with Robber Baron or Pick Pcket Scanners can steal cargo from planets and fleets owned by other people
            if (dest.Owned && dest.PlayerNum != source.PlayerNum)
            {
                if (source is Fleet fleet)
                {
                    if (dest is Fleet)
                    {
                        // we can steal from fleets, go!
                        return GetCanStealFleetCargo(fleet, mapObjectsByLocation);
                    }
                    else if (dest is Planet)
                    {
                        // we are a fleet with cargo stealing capabiliities, go!
                        return GetCanStealPlanetCargo(fleet, mapObjectsByLocation);
                    }
                    else
                    {
                        // we can always steal from enemy salvage, mineral packets, etc
                        return true;
                    }
                }

                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Transfer cargo from the source to the dest. Anything the dest can't take is put back in the source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="cargo"></param>
        /// <param name="fuel"></param>
        /// <returns>The actual cargo transferred</returns>
        public CargoTransferResult Transfer(ICargoHolder source, ICargoHolder dest, Cargo cargo, int fuel)
        {
            // first take away from the source
            var sourceResult = source.Transfer(-cargo, -fuel);

            // The sourceResult is how much we successfully took from the source, so invert that and give it to the dest
            var destResult = dest.Transfer(-sourceResult.cargo, -sourceResult.fuel);

            // give back any difference we couldn't give to the dest, to the source
            var diff = sourceResult + destResult;
            source.Transfer(-diff.cargo, -diff.fuel);

            return sourceResult - diff;
        }

    }
}

