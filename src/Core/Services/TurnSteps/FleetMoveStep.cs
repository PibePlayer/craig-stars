using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static CraigStars.Utils.Utils;

namespace CraigStars
{
    /// <summary>
    /// Move Fleets in space
    /// </summary>
    public class FleetMoveStep : TurnGenerationStep
    {
        private readonly IRulesProvider rulesProvider;
        private readonly MineFieldDamager mineFieldDamager;
        private readonly ShipDesignDiscoverer designDiscoverer;
        private readonly FleetService fleetService;
        private readonly PlayerService playerService;

        private Rules Rules => rulesProvider.Rules;

        public FleetMoveStep(IProvider<Game> gameProvider, IRulesProvider rulesProvider, MineFieldDamager mineFieldDamager, ShipDesignDiscoverer designDiscoverer, FleetService fleetService, PlayerService playerService) : base(gameProvider, TurnGenerationState.FleetMoveStep)
        {
            this.rulesProvider = rulesProvider;
            this.mineFieldDamager = mineFieldDamager;
            this.designDiscoverer = designDiscoverer;
            this.fleetService = fleetService;
            this.playerService = playerService;
        }

        public override void Process()
        {
            Game.Fleets.ForEach(fleet => Move(fleet, Game.Players[fleet.PlayerNum]));
        }

        /// <summary>
        /// Move a fleet
        /// </summary>
        /// <param name="fleet"></param>
        internal void Move(Fleet fleet, Player player)
        {
            if (fleet.Waypoints.Count > 1)
            {
                Waypoint wp0 = fleet.Waypoints[0];
                Waypoint wp1 = fleet.Waypoints[1];
                float totalDist = fleet.Position.DistanceTo(wp1.Position);

                // remove the fleet from the list of map objects at it's current location
                var originalPosition = fleet.Position;

                if (wp1.WarpFactor == Waypoint.StargateWarpFactor)
                {
                    // yeah, gate!
                    GateFleet(fleet, player, wp0, wp1, totalDist);
                }
                else
                {
                    MoveFleet(fleet, player, wp0, wp1, totalDist);
                }

                // update the game dictionaries with this fleet's new position
                Game.MoveMapObject(fleet, originalPosition, fleet.Position);
            }
            else
            {
                fleet.PreviousPosition = fleet.Position;
                fleet.WarpSpeed = 0;
                fleet.Heading = Vector2.Zero;
            }
        }

        /// <summary>
        /// Move the fleet the cool way, with stargates!
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="wp0"></param>
        /// <param name="wp1"></param>
        /// <param name="totalDist"></param>
        internal void GateFleet(Fleet fleet, Player player, Waypoint wp0, Waypoint wp1, float totalDist)
        {
            // if we got here, both source and dest have stargates
            var sourcePlanet = fleet.Orbiting;
            var destPlanet = wp1.Target as Planet;

            if (sourcePlanet == null || !sourcePlanet.HasStargate)
            {
                Message.FleetStargateInvalidSource(player, fleet, wp0);
                return;
            }
            if (destPlanet == null || !destPlanet.HasStargate)
            {
                Message.FleetStargateInvalidDest(player, fleet, wp0, wp1);
                return;
            }

            var sourcePlanetPlayer = Game.Players[sourcePlanet.PlayerNum];
            var destPlanetPlayer = Game.Players[destPlanet.PlayerNum];

            if (!sourcePlanetPlayer.IsFriend(player.Num))
            {
                Message.FleetStargateInvalidSourceOwner(player, fleet, wp0, wp1);
                return;
            }
            if (!destPlanetPlayer.IsFriend(player.Num))
            {
                Message.FleetStargateInvalidDestOwner(player, fleet, wp0, wp1);
                return;
            }
            if (fleet.Cargo.Colonists > 0 && !sourcePlanet.OwnedBy(player))
            {
                Message.FleetStargateInvalidColonists(player, fleet, wp0, wp1);
                return;
            }

            var sourceStargate = sourcePlanet.Starbase.Spec.Stargate;
            var destStargate = destPlanet.Starbase.Spec.Stargate;

            // only the source gate matters for range
            var minSafeRange = sourceStargate.SafeRange;
            var minSafeHullMass = Math.Min(sourceStargate.SafeHullMass, destStargate.SafeHullMass);

            // check if we are exceeding the max distance
            if (totalDist > minSafeRange * Game.Rules.StargateMaxRangeFactor)
            {
                Message.FleetStargateInvalidRange(player, fleet, wp0, wp1, totalDist);
                return;
            }

            // check if any ships exceed the max mass allowed
            foreach (var token in fleet.Tokens)
            {
                if (token.Design.Spec.Mass > minSafeHullMass * Game.Rules.StargateMaxHullMassFactor)
                {
                    Message.FleetStargateInvalidMass(player, fleet, wp0, wp1);
                    return;
                }
            }

            // dump cargo if we aren't IT
            if (fleet.Cargo.Total > 0 && !player.Race.Spec.CanGateCargo)
            {
                Message.FleetStargateDumpedCargo(player, fleet, wp0, wp1, fleet.Cargo);
                fleet.Transfer(-fleet.Cargo);
                sourcePlanet.Transfer(fleet.Cargo);
            }

            // apply overgate damage and delete tokens (and possibly the fleet)
            // also vanish tokens for non IT races
            ApplyOvergatePenalty(fleet, player, totalDist, wp0, wp1, sourceStargate, destStargate);

            if (fleet.Tokens.Count > 0)
            {
                // if we survived, warp it!
                CompleteMove(fleet, wp0, wp1);
            }
        }

        /// <summary>
        /// Move the fleet the old fashioned way, with engines
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="wp0"></param>
        /// <param name="wp1"></param>
        /// <param name="totalDist"></param>
        internal void MoveFleet(Fleet fleet, Player player, Waypoint wp0, Waypoint wp1, float totalDist)
        {
            fleet.PreviousPosition = fleet.Position;
            float dist = wp1.WarpFactor * wp1.WarpFactor;

            // check for CE engine failure
            if (player.Race.Spec.EngineFailureRate > 0 && wp1.WarpFactor > player.Race.Spec.EngineReliableSpeed
                && player.Race.Spec.EngineFailureRate >= (float)Rules.Random.NextDouble())
            {
                Message.FleetEngineFailure(player, fleet);
                return;
            }

            // go with the lower
            if (totalDist < dist)
            {
                dist = totalDist;
            }

            // get the cost for the fleet
            int fuelCost = fleetService.GetFuelCost(fleet, player, wp1.WarpFactor, dist);
            int fuelGenerated = 0;
            if (fuelCost > fleet.Fuel)
            {
                // we will run out of fuel
                // if this distance would have cost us 10 fuel but we have 6 left, only travel 60% of the distance.
                var distanceFactor = fleet.Fuel / fuelCost;
                dist = dist * distanceFactor;

                // collide with minefields on route, but don't hit a minefield if we run out of fuel beforehand
                dist = CheckForMineFields(fleet, player, wp1, dist);

                fleet.Fuel = 0;
                wp1.WarpFactor = fleetService.GetNoFuelWarpFactor(fleet, player);
                Message.FleetOutOfFuel(player, fleet, wp1.WarpFactor);

                // if we ran out of fuel 60% of the way to our normal distance, the remaining 40% of our time
                // was spent travelling at fuel generation speeds:
                var remainingDistanceTravelled = (1 - distanceFactor) * (wp1.WarpFactor * wp1.WarpFactor);
                dist += remainingDistanceTravelled;
                fuelGenerated = fleetService.GetFuelGeneration(fleet, player, wp1.WarpFactor, remainingDistanceTravelled);
            }
            else
            {
                // collide with minefields on route, but don't hit a minefield if we run out of fuel beforehand
                var actualDist = CheckForMineFields(fleet, player, wp1, dist);
                if (actualDist != dist)
                {
                    dist = actualDist;
                    fuelCost = fleetService.GetFuelCost(fleet, player, wp1.WarpFactor, dist);
                    // we hit a minefield, update fuel usage
                }

                fleet.Fuel -= fuelCost;
                fuelGenerated = fleetService.GetFuelGeneration(fleet, player, wp1.WarpFactor, dist);
            }

            // message the player about fuel generation
            fuelGenerated = Math.Min(fuelGenerated, fleet.FuelMissing);
            if (fuelGenerated > 0)
            {
                fleet.Fuel += fuelGenerated;
                Message.FleetGeneratedFuel(player, fleet, fuelGenerated);
            }

            // assuming we move at all, make sure we are no longer orbiting any planets
            if (dist > 0 && fleet.Orbiting != null)
            {
                fleet.Orbiting = null;
            }

            if (wp0.OriginalTarget == null || !wp0.OriginalPosition.HasValue)
            {
                wp0.OriginalTarget = wp0.Target;
                wp0.OriginalPosition = fleet.Position;
            }

            if (totalDist == dist)
            {
                CompleteMove(fleet, wp0, wp1);
            }
            else
            {
                // move this fleet closer to the next waypoint
                fleet.WarpSpeed = wp1.WarpFactor;
                fleet.Heading = (wp1.Position - fleet.Position).Normalized();
                wp0.Target = null;

                fleet.Position += fleet.Heading * dist;
                wp0.Position = fleet.Position;
            }
        }

        /// <summary>
        /// Complete a move from one waypoint to another
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="wp0"></param>
        /// <param name="wp1"></param>
        void CompleteMove(Fleet fleet, Waypoint wp0, Waypoint wp1)
        {
            fleet.Position = wp1.Position;

            // find out if we arrived at a planet, either by reaching our target fleet 
            // or reaching a planet
            Planet targetPlanet = wp1.Target as Planet;
            if (targetPlanet == null && wp1.Target is Fleet targetFleet && targetFleet.Orbiting != null)
            {
                targetPlanet = targetFleet.Orbiting;
            }

            if (targetPlanet != null)
            {
                fleet.Orbiting = targetPlanet;
                if (fleet.PlayerNum == targetPlanet.PlayerNum && targetPlanet.HasStarbase)
                {
                    // refuel at starbases
                    fleet.Fuel = fleet.Spec.FuelCapacity;
                }
            }
            else if (wp1.Target is Wormhole wormhole)
            {
                fleet.Position = wormhole.Destination.Position;
            }

            // remove the previous waypoint, it's been processed already
            if (fleet.RepeatOrders)
            {
                var wpToRepeat = fleet.Waypoints[0];
                wpToRepeat.Target = wpToRepeat.OriginalTarget;
                wpToRepeat.OriginalPosition = wpToRepeat.OriginalPosition;
                // if we are supposed to repeat orders, 
                fleet.Waypoints.Add(wpToRepeat);
            }

            if (!wp0.WaitAtWaypoint)
            {
                fleet.Waypoints.RemoveAt(0);
            }

            // we arrived, process the current task (the previous waypoint)
            if (fleet.Waypoints.Count == 1)
            {
                fleet.WarpSpeed = 0;
                fleet.Heading = Vector2.Zero;
            }
            else
            {
                wp1 = fleet.Waypoints[1];
                fleet.WarpSpeed = wp1.WarpFactor;
                fleet.Heading = (wp1.Position - fleet.Position).Normalized();
            }
        }

        /// <summary>
        /// Apply damage (if any) to each token that overgated
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="sourceStargate"></param>
        /// <param name="destStargate"></param>
        internal void ApplyOvergatePenalty(Fleet fleet, Player player, float distance, Waypoint wp0, Waypoint wp1, TechHullComponent sourceStargate, TechHullComponent destStargate)
        {
            int totalDamage = 0;
            int shipsLostToDamage = 0;
            int shipsLostToTheVoid = 0;
            int startingShips = 0;
            foreach (var token in fleet.Tokens)
            {
                startingShips += token.Quantity;
                // Inner stellar travellers never lose ships to the void, but everyone else does
                if (player.Race.Spec.ShipsVanishInVoid)
                {
                    var rangeVanishChance = token.GetStargateRangeVanishingChance(distance, sourceStargate.SafeRange);
                    var massVanishingChance = token.GetStargateMassVanishingChance(sourceStargate.SafeHullMass, Game.Rules.StargateMaxHullMassFactor);
                    // Combined vanishing chance idea courtesy of ekolis
                    var vanishingChance = 1 - (1 - rangeVanishChance) * (1 - massVanishingChance);

                    if (rangeVanishChance > 0 || massVanishingChance > 0)
                    {
                        for (int i = 0; i < token.Quantity; i++)
                        {
                            // check if it vanishes due to range, if not, check if it vanishes due 
                            // to mass. Each ship can only vanish once
                            if (vanishingChance > Game.Rules.Random.Next())
                            {
                                // oh no, we lost a ship!
                                shipsLostToTheVoid++;
                                token.Quantity--;
                                i--;
                                if (token.QuantityDamaged > 0)
                                {
                                    // get rid of the damaged ships first and redistribute the damage
                                    // i.e. if we have 2 damaged ships with 20 total damage
                                    // we get rid of one of them and leave one with 10 damage
                                    token.Damage = Math.Max(0, (float)token.Damage / token.QuantityDamaged);
                                    token.QuantityDamaged--;
                                    // can't have damage without damaged ships
                                    // I don't think this should ever come up
                                    if (token.QuantityDamaged == 0)
                                    {
                                        token.Damage = 0;
                                    }
                                }
                            }
                        }
                    }
                }

                // if we didn't lose tokens in 
                if (token.Quantity > 0)
                {
                    var tokenDamage = token.ApplyOvergateDamage(distance, sourceStargate.SafeRange, sourceStargate.SafeHullMass, destStargate.SafeHullMass, Game.Rules.StargateMaxHullMassFactor);

                    totalDamage += tokenDamage.damage;
                    shipsLostToDamage += tokenDamage.shipsDestroyed;
                }
            }

            // remove any tokens that were lost completely
            fleet.Tokens = fleet.Tokens.Where(token => token.Quantity > 0).ToList();

            if (fleet.Tokens.Count == 0)
            {
                EventManager.PublishMapObjectDeletedEvent(fleet);
                Message.FleetStargateDestroyed(player, fleet, wp0, wp1);
            }
            else
            {
                if (totalDamage > 0 || shipsLostToTheVoid > 0)
                {
                    Message.FleetStargateDamaged(player, fleet, wp0, wp1, totalDamage, startingShips, shipsLostToDamage, shipsLostToTheVoid);
                }
            }
        }


        /// <summary>
        /// Check for mine field collisions. If we collide with one, do damage and stop the fleet
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="dest"></param>
        /// <param name="distance"></param>
        /// <returns>The actual distance travelled, if stopped by a minefield</returns>
        internal float CheckForMineFields(Fleet fleet, Player player, Waypoint dest, float distance)
        {
            int safeWarpBonus = player.Race.Spec.MineFieldSafeWarpBonus;

            // see if we are colliding with any of these minefields
            foreach (var mineField in Game.MineFields.Where(mf => mf.PlayerNum != fleet.PlayerNum))
            {
                // we only check if we are going faster than allowed by the minefield.
                var stats = Game.Rules.MineFieldStatsByType[mineField.Type];
                if (dest.WarpFactor > stats.MaxSpeed + safeWarpBonus)
                {
                    // this is not our minefield, and we are going fast, check if we intersect.
                    Vector2 from = fleet.Position;
                    Vector2 to = (dest.Position - fleet.Position).Normalized() * distance + from;
                    float collision = SegmentIntersectsCircle(from, to, mineField.Position, mineField.Radius);
                    if (collision == -1)
                    {
                        // miss! phew, that was close!
                        return distance;
                    }
                    else
                    {
                        // we are travelling through this minefield, for each light year we go through, check for a hit
                        // collision is 0 to 1, which is the percent of our travel segment that is NOT in the field.
                        // figure out what that is in lightYears
                        // if we are travelling 32 light years and 3/4 of it is through the minefield, we need to check
                        // for collision 24 times
                        int lightYearsInField = (int)Math.Min(mineField.Radius, Math.Ceiling((1 - collision) * distance));
                        float lightYearsBeforeField = collision * distance;

                        // Each type of minefield has a chance to hit based on how fast
                        // the fleet is travelling through the field. A normal mine has a .3% chance
                        // of hitting a ship per extra warp over warp 4, so a warp 9 ship
                        // has a 1.5% chance of hitting a mine per lightyear travelled
                        int unsafeWarp = dest.WarpFactor - (stats.MaxSpeed + safeWarpBonus);
                        float chanceToHit = stats.ChanceOfHit * unsafeWarp;
                        for (int checkNum = 0; checkNum < lightYearsInField; checkNum++)
                        {
                            if (chanceToHit >= Game.Rules.Random.NextDouble())
                            {
                                // ouch, we hit a minefield!
                                // we stop moving at the hit, so if we made it 8 checks out of 24 for our above example
                                // we only travel 8 lightyears through the field (plus whatever distance we travelled to get to the field)
                                var actualDistanceTravelled = lightYearsBeforeField + checkNum;
                                var mineFieldPlayer = Game.Players[mineField.PlayerNum];
                                var fleetPlayer = Game.Players[fleet.PlayerNum];

                                mineFieldDamager.TakeMineFieldDamage(fleet, fleetPlayer, mineField, mineFieldPlayer, stats);
                                mineFieldDamager.ReduceMineFieldOnImpact(mineField);
                                if (mineFieldPlayer.Race.Spec.MineFieldsAreScanners)
                                {
                                    // SD races discover the exact fleet makeup
                                    foreach (var token in fleet.Tokens)
                                    {
                                        designDiscoverer.Discover(mineFieldPlayer, token.Design, true);
                                    }
                                }
                                return actualDistanceTravelled;
                            }
                        }
                    }
                }
            }

            return distance;

        }


    }
}