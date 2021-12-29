using System;
using System.Collections.Generic;
using CraigStars.Utils;
using Godot;
using Newtonsoft.Json;

namespace CraigStars
{
    public class Message
    {
        public MessageType Type { get; set; }
        public string Text { get; set; }

        public Guid? TargetGuid { get; set; }

        public Guid? BattleGuid { get; set; }

        public Message() { }

        [JsonConstructor]
        public Message(MessageType type, string text, Guid? target, Guid? battleGuid)
        {
            Type = type;
            Text = text;
            TargetGuid = target;
            BattleGuid = battleGuid;
        }

        public Message(MessageType type, string text, MapObject target)
        {
            Type = type;
            Text = text;
            TargetGuid = target?.Guid;
        }

        public Message(MessageType type, string text)
        {
            Type = type;
            Text = text;
        }

        public static void Info(Player player, string text)
        {
            player.Messages.Add(new Message(MessageType.Info, text));
        }

        public static void HomePlanet(Player player, Planet planet)
        {
            string text = $"Your home planet is {planet.Name}. Your people are ready to leave the nest and explore the universe.  Good luck.";
            player.Messages.Add(new Message(MessageType.HomePlanet, text, planet));
        }

        public static void PlayerDiscovered(Player player, Player otherPlayer)
        {
            string text = $"You have discovered a new species, the {otherPlayer.Race.PluralName}. You are not alone in the universe!";
            player.Messages.Insert(0, new Message(MessageType.PlayerDiscovery, text));
        }

        public static void Mine(Player player, Planet planet, int numMines)
        {
            string text = $"You have built {numMines} mine(s) on {planet.Name}.";
            player.Messages.Add(new Message(MessageType.BuiltMine, text, planet));
        }

        public static void Factory(Player player, Planet planet, int numFactories)
        {
            string text = $"You have built {numFactories} factory(s) on {planet.Name}.";
            player.Messages.Add(new Message(MessageType.BuiltFactory, text, planet));

        }

        public static void Defense(Player player, Planet planet, int numDefenses)
        {
            string text = $"You have built {numDefenses} defense(s) on {planet.Name}.";
            player.Messages.Add(new Message(MessageType.BuiltDefense, text, planet));

        }

        public static void Terraform(Player player, Planet planet, HabType habType, int change)
        {
            string changeText = change > 0 ? "increased" : "decreased";
            string newValueText = "";
            var newValue = planet.Hab.Value[habType];
            switch (habType)
            {
                case HabType.Gravity:
                    newValueText = TextUtils.GetGravString(newValue);
                    break;
                case HabType.Temperature:
                    newValueText = TextUtils.GetTempString(newValue);
                    break;
                case HabType.Radiation:
                    newValueText = TextUtils.GetRadString(newValue);
                    break;
            }

            string text = $"Your terraforming efforts on {planet.Name} have {changeText} the {habType} to {newValueText}";
            player.Messages.Add(new Message(MessageType.BuiltTerraform, text, planet));
        }

        public static void PacketTerraform(Player player, Planet planet, HabType habType, int change)
        {
            string changeText = change > 0 ? "increased" : "decreased";
            string newValueText = "";
            var newValue = planet.Hab.Value[habType];
            switch (habType)
            {
                case HabType.Gravity:
                    newValueText = TextUtils.GetGravString(newValue);
                    break;
                case HabType.Temperature:
                    newValueText = TextUtils.GetTempString(newValue);
                    break;
                case HabType.Radiation:
                    newValueText = TextUtils.GetRadString(newValue);
                    break;
            }

            string text = $"Your mineral packet hitting {planet.Name} has {changeText} the {habType} to {newValueText}";
            player.Messages.Add(new Message(MessageType.PacketTerraform, text, planet));
        }

        public static void PacketPermaform(Player player, Planet planet, HabType habType, int change)
        {
            string changeText = change > 0 ? "increased" : "decreased";
            string text = $"Your mineral packet has permanently {changeText} the {habType} on {planet.Name}.";
            player.Messages.Add(new Message(MessageType.PacketPermaform, text, planet));
        }

        public static void Instaform(Player player, Planet planet, Hab terraformAmount)
        {
            string text = $"Your race has instantly terraformed {planet.Name} up to optimal conditions.";
            player.Messages.Add(new Message(MessageType.Instaform, text, planet));
        }

        public static void Permaform(Player player, Planet planet, HabType habType, int change)
        {
            string changeText = change > 0 ? "increased" : "decreased";
            string text = $"Your race has permanently {changeText} the {habType} on {planet.Name}.";
            player.Messages.Add(new Message(MessageType.Permaform, text, planet));
        }

        public static void MineralPacket(Player player, Planet planet, MineralPacket packet)
        {
            string text = $"{planet.Name} has produced a mineral packet which has a destination of {packet.Target.Name}";
            player.Messages.Add(new Message(MessageType.BuiltMineralPacket, text, planet));

        }

        public static void MineralPacketArrived(Player player, Planet planet, MineralPacket packet)
        {
            string text = $"Your mineral packet containing {packet.Cargo.Total}kT of minerals has landed at {planet.Name}.";
            player.Messages.Add(new Message(MessageType.MineralPacketLanded, text, planet));
        }

        public static void MineralPacketCaught(Player player, Planet planet, MineralPacket packet)
        {
            string text = $"Your mass accelerator at {planet.Name} has successfully captured a packet containing {packet.Cargo.Total}kT of minerals.";
            player.Messages.Add(new Message(MessageType.MineralPacketCaught, text, planet));
        }

        public static void MineralPacketDamage(Player player, Planet planet, MineralPacket packet, int colonistsKilled, int defensesDestroyed)
        {
            string text = "";
            if (planet.HasStarbase && planet.Starbase.Spec.HasMassDriver)
            {
                if (defensesDestroyed == 0)
                {
                    text = $"Your mass accelerator at {planet.Name} was partially successful in capturing a {packet.Cargo.Total}kT mineral packet. Unable to completely slow the packet, {colonistsKilled} of your colonists were killed in the collision.";
                }
                else
                {
                    text = $"Your mass accelerator at {planet.Name} was partially successful in capturing a {packet.Cargo.Total}kT mineral packet. Unfortunately, {colonistsKilled} of your colonists and {defensesDestroyed} of your defenses were destroyed in the collision.";
                }
            }
            else
            {
                if (planet.Population == 0)
                {
                    text = $"{planet.Name} was annihilated by a mineral packet.  All of your colonists were killed.";
                }
                else if (defensesDestroyed == 0)
                {
                    text = $"{planet.Name} was bombarded with a {packet.Cargo.Total}kT mineral packet. {colonistsKilled} of your colonists were killed by the collision.";
                }
                else
                {
                    text = $"{planet.Name} was bombarded with a {packet.Cargo.Total}kT mineral packet. {colonistsKilled} of your colonists and {defensesDestroyed} of your defenses were destroyed by the collision.";
                }

            }
            player.Messages.Add(new Message(MessageType.MineralPacketDamage, text, planet));
        }

        public static void BuildMineralPacketNoMassDriver(Player player, Planet planet)
        {
            string text = $"You have attempted to build a mineral packate on {planet.Name}, but you have no Starbase equipped with a mass driver on this planet. Production for this planet has been cancelled.";
            player.Messages.Add(new Message(MessageType.Invalid, text, planet));
        }

        public static void BuildMineralPacketNoTarget(Player player, Planet planet)
        {
            string text = $"You have attempted to build a mineral packate on {planet.Name}, but you have not specified a target. The minerals have been returned to the planet and production has been cancelled.";
            player.Messages.Add(new Message(MessageType.Invalid, text, planet));

        }

        public static void TechLevel(Player player, TechField field, int level, TechField nextField)
        {
            string text = $"Your scientists have completed research into Tech Level {level} for {field}.  They will continue their efforts in the {nextField} field.";
            player.Messages.Add(new Message(MessageType.GainTechLevel, text));
        }

        public static void ColonizeNonPlanet(Player player, Fleet fleet)
        {
            string text = $"{fleet.Name} has attempted to colonize a waypoint with no Planet.";
            player.Messages.Add(new Message(MessageType.Invalid, text, fleet));
        }

        public static void ColonizeOwnedPlanet(Player player, Fleet fleet)
        {
            string text = $"{fleet.Name} has attempted to colonize a planet that is already inhabited.";
            player.Messages.Add(new Message(MessageType.Invalid, text, fleet));

        }

        public static void ColonizeWithNoModule(Player player, Fleet fleet)
        {
            string text = $"{fleet.Name} has attempted to colonize a planet without a colonization module.";
            player.Messages.Add(new Message(MessageType.Invalid, text, fleet));

        }

        public static void ColonizeWithNoColonists(Player player, Fleet fleet)
        {
            string text = $"{fleet.Name} has attempted to colonize a planet without bringing any colonists.";
            player.Messages.Add(new Message(MessageType.Invalid, text, fleet));
        }

        public static void PlanetColonized(Player player, Planet planet)
        {
            string text = $"Your colonists are now in control of {planet.Name}";
            player.Messages.Add(new Message(MessageType.PlanetColonized, text, planet));
        }

        public static void FleetEngineFailure(Player player, Fleet fleet)
        {
            var text = $"{fleet.Name} was unable to engage it's engines due to balky equipment. Engineers think they have the problem fixed for the time being.";
            player.Messages.Add(new Message(MessageType.FleetEngineFailure, text, fleet));
        }

        public static void FleetOutOfFuel(Player player, Fleet fleet, int warpFactor)
        {
            string text = $"{fleet.Name} has run out of fuel. The fleet's speed has been decreased to Warp {warpFactor}.";
            player.Messages.Add(new Message(MessageType.FleetOutOfFuel, text, fleet));
        }

        public static void FleetGeneratedFuel(Player player, Fleet fleet, int fuelGenerated)
        {
            string text = $"{fleet.Name}'s ram scoops have produced {fuelGenerated}mg of fuel from interstellar hydrogen.";
            player.Messages.Add(new Message(MessageType.FleetGeneratedFuel, text, fleet));
        }

        public static void FleetScrapped(Player player, Fleet fleet, int num_minerals, Planet planet)
        {
            string text = $"{fleet.Name} has been dismantled for {num_minerals}kT of minerals which have been deposited on {planet.Name}.";
            player.Messages.Add(new Message(MessageType.FleetScrapped, text, planet));
        }

        public static void FleetMerged(Player player, Fleet fleet, Fleet mergedInto)
        {
            string text = $"{fleet.Name} has been merged into {mergedInto}.";
            player.Messages.Add(new Message(MessageType.FleetMerged, text, mergedInto));
        }

        public static void FleetInvalidMergeNotFleet(Player player, Fleet fleet)
        {
            string text = $"{fleet.Name} was unable to complete it's merge orders as the waypoint destination wasn't a fleet.";
            player.Messages.Add(new Message(MessageType.FleetInvalidMergeNotFleet, text, fleet));
        }

        public static void FleetInvalidMergeNotOwned(Player player, Fleet fleet)
        {
            string text = $"{fleet.Name} was unable to complete it's merge orders as the destination fleet wasn't one of yours.";
            player.Messages.Add(new Message(MessageType.FleetInvalidMergeUnowned, text, fleet));
        }

        public static void FleetPatrolTargeted(Player player, Fleet fleet, Fleet target)
        {
            player.Messages.Add(new Message(MessageType.FleetPatrolTargeted, $"Your patrolling {fleet.Name} has targeted {target.Name} for intercept.", fleet));
        }

        public static void FleetInvalidRouteNotPlanet(Player player, Fleet fleet)
        {
            string text = $"{fleet.Name} could not be routed because it is not at a planet.";
            player.Messages.Add(new Message(MessageType.FleetInvalidRouteNotPlanet, text, fleet));
        }

        public static void FleetInvalidRouteNotFriendlyPlanet(Player player, Fleet fleet, Planet planet)
        {
            string text = $"{fleet.Name} could not be routed because you are not friends with the owners of {planet.Name}";
            player.Messages.Add(new Message(MessageType.FleetInvalidRouteNotFriendlyPlanet, text, fleet));
        }

        public static void FleetInvalidRouteNoRouteTarget(Player player, Fleet fleet, Planet planet)
        {
            string text = $"{fleet.Name} could not be routed because {planet.Name} has no route set.";
            player.Messages.Add(new Message(MessageType.FleetInvalidRouteNoRouteTarget, text, fleet));
        }

        public static void FleetRouted(Player player, Fleet fleet, Planet planet, MapObject routeTarget)
        {
            string text = $"{fleet.Name} has been routed by the citizens of {planet.Name} to {routeTarget.Name}";
            player.Messages.Add(new Message(MessageType.FleetRoute, text, fleet));
        }

        public static void FleetBuilt(Player player, ShipDesign design, Fleet fleet, int numBuilt)
        {
            string text;
            if (numBuilt == 1)
            {
                if (design.Hull.Starbase)
                {
                    text = $"You have built a new {design.Name} on {fleet.Orbiting.Name}.";
                }
                else
                {
                    text = $"Your starbase at {fleet.Orbiting.Name} has built a new {design.Name}.";
                }
            }
            else
            {
                text = $"Your starbase at {fleet.Orbiting.Name} has built {numBuilt} new {design.Name}s.";
            }
            player.Messages.Add(new Message(MessageType.BuiltShip, text, fleet));
        }

        public static void FleetBuiltForComposition(Player player, ShipDesign design, Fleet fleet, int numBuilt)
        {
            string text;
            if (numBuilt == 1)
            {
                text = $"Your starbase at {fleet.Orbiting.Name} has built a new {design.Name}. It has been added to {fleet.Name}.";
            }
            else
            {
                text = $"Your starbase at {fleet.Orbiting.Name} has built {numBuilt} new {design.Name}s. They have been added to {fleet.Name}.";
            }
            player.Messages.Add(new Message(MessageType.BuiltShip, text, fleet));
        }

        public static void FleetStargateInvalidSource(Player player, Fleet fleet, Waypoint wp0)
        {
            player.Messages.Add(new Message(MessageType.Invalid,
                $"{fleet.Name} attempted to use a stargate at {wp0.TargetName}, but no stargate exists there.",
                fleet)
            );
        }

        public static void FleetStargateInvalidSourceOwner(Player player, Fleet fleet, Waypoint wp0, Waypoint wp1)
        {
            player.Messages.Add(new Message(MessageType.Invalid,
                $"{fleet.Name} attempted to use a stargate at {wp0.TargetName}, but could not because the starbase is not owned by you or a friend of yours.",
                fleet)
            );
        }

        public static void FleetStargateInvalidDest(Player player, Fleet fleet, Waypoint wp0, Waypoint wp1)
        {
            player.Messages.Add(new Message(MessageType.Invalid,
                $"{fleet.Name} attempted to use a stargate at {wp0.TargetName} to reach {wp1.TargetName}, but no stargate could be detected at the destination.",
                fleet)
            );
        }

        public static void FleetStargateInvalidDestOwner(Player player, Fleet fleet, Waypoint wp0, Waypoint wp1)
        {
            player.Messages.Add(new Message(MessageType.Invalid,
                $"{fleet.Name} attempted to use a stargate at {wp0.TargetName} to reach {wp1.TargetName}, but could not because the destination starbase is not owned by you or a friend of yours.",
                fleet)
            );
        }

        public static void FleetStargateInvalidRange(Player player, Fleet fleet, Waypoint wp0, Waypoint wp1, float totalDist)
        {
            player.Messages.Add(new Message(MessageType.Invalid,
                $"{fleet.Name} attempted to use a stargate at {wp0.TargetName} to reach {wp1.TargetName}, but the distance of {totalDist:.#} l.y. was outside the max range of the stargates.",
                fleet)
            );
        }

        public static void FleetStargateInvalidMass(Player player, Fleet fleet, Waypoint wp0, Waypoint wp1)
        {
            player.Messages.Add(new Message(MessageType.Invalid,
                $"{fleet.Name} attempted to use a stargate at {wp0.TargetName} to reach {wp1.TargetName}, but your ships are too massive.",
                fleet)
            );
        }

        public static void FleetStargateInvalidColonists(Player player, Fleet fleet, Waypoint wp0, Waypoint wp1)
        {
            player.Messages.Add(new Message(MessageType.Invalid,
                $"{fleet.Name} attempted to use a stargate at {wp0.TargetName} to reach {wp1.TargetName}, but you are carrying colonists and can't drop them off as you don't own the planet.",
                fleet)
            );
        }

        public static void FleetStargateDumpedCargo(Player player, Fleet fleet, Waypoint wp0, Waypoint wp1, Cargo cargo)
        {
            string text = "";
            if (cargo.HasColonists && cargo.HasMinerals)
            {
                text = $"{fleet.Name} has unloaded {cargo.Colonists * 100} colonists and {cargo.Total - cargo.Colonists}kT of minerals in preparation for jumping through the stargate at {wp0.TargetName} to reach {wp1.TargetName}.";
            }
            else if (cargo.HasColonists)
            {
                text = $"{fleet.Name} has unloaded {cargo.Colonists * 100} colonists in preparation for jumping through the stargate at {wp0.TargetName} to reach {wp1.TargetName}.";
            }
            else
            {
                text = $"{fleet.Name} has unloaded {cargo.Total}kT of minerals in preparation for jumping through the stargate at {wp0.TargetName} to reach {wp1.TargetName}.";
            }
            player.Messages.Add(new Message(MessageType.Invalid,
                text,
                fleet)
            );
        }

        public static void FleetStargateDestroyed(Player player, Fleet fleet, Waypoint wp0, Waypoint wp1)
        {
            player.Messages.Add(new Message(MessageType.FleetStargateDamaged,
                $"Heedless to the danger, {fleet.Name} attempted to use the stargate at {wp0.TargetName} to reach {wp0.TargetName}.  The fleet never arrived. The distance or mass must have been too great.",
                fleet)
            );
        }

        public static void FleetStargateDamaged(Player player, Fleet fleet, Waypoint wp0, Waypoint wp1, int damage, int startingShips, int shipsLostToDamage, int shipsLostToTheVoid)
        {
            var totalShipsLost = shipsLostToDamage + shipsLostToTheVoid;
            string text = "";
            if (totalShipsLost == 0)
            {
                text = $"{fleet.Name} used the stargate at {wp0.TargetName} to reach {wp1.TargetName} losing no ships but suffering {damage} dp of damage.  They exceeded the capability of the gates.";
            }
            else if (totalShipsLost < 5)
            {
                text = $"{fleet.Name} used the stargate at {wp0.TargetName} to reach {wp1.TargetName} losing only {totalShipsLost} ship{(totalShipsLost == 1 ? "" : "s")} to the treacherous void.  They were fortunate.  They exceeded the capability of the gates.";
            }
            else if (totalShipsLost >= 5 && totalShipsLost <= 10)
            {
                text = $"{fleet.Name} used the stargate at {wp0.TargetName} to reach {wp1.TargetName} losing {totalShipsLost} ships to the unforgiving void. Exceeding the capability of your stargates is not recommended.";
            }
            else if (totalShipsLost >= 10 && totalShipsLost <= 50)
            {
                text = $"{fleet.Name} used the stargate at {wp0.TargetName} to reach {wp1.TargetName} unfortunately losing {totalShipsLost} ships to the  great unknown. Exceeding the capability of your stargates is dangerous.";
            }
            else if (totalShipsLost >= 50)
            {
                text = $"{fleet.Name} used the stargate at {wp0.TargetName} to reach {wp1.TargetName} losing an unbelievable {totalShipsLost} ships. The jump was far in excess of the capabilities of starbases involved..";
            }
            player.Messages.Add(new Message(MessageType.FleetStargateDamaged,
                text,
                fleet)
            );

        }

        public static void FleetTransportedCargo(Player player, Fleet source, CargoType cargoType, ICargoHolder dest, int transferAmount)
        {
            string text = "";
            if (cargoType == CargoType.Colonists)
            {
                if (transferAmount < 0)
                {
                    text = $"{source.Name} has beamed {-transferAmount * 100} {cargoType} from {dest.Name}";
                }
                else
                {
                    text = $"{source.Name} has beamed {transferAmount * 100} {cargoType} to {dest.Name}";
                }
            }
            else
            {
                if (transferAmount < 0)
                {
                    text = $"{source.Name} has loaded {-transferAmount} {cargoType} from {dest.Name}";
                }
                else
                {
                    text = $"{source.Name} has unloaded {transferAmount} {cargoType} to {dest.Name}";
                }
            }
            player.Messages.Add(new Message(MessageType.CargoTransferred, text, source));
        }

        public static void RemoteMineNoMiners(Player player, Fleet fleet, Planet planet)
        {
            player.Messages.Add(new Message(MessageType.Invalid,
                $"{fleet.Name} had orders to mine {planet.Name}, but the fleet doesn't have any remote mining modules. The order has been canceled.", fleet));
        }

        public static void RemoteMineInhabited(Player player, Fleet fleet, Planet planet)
        {
            player.Messages.Add(new Message(MessageType.Invalid,
                $"Remote mining robots from {fleet.Name} had orders to mine {planet.Name}, but the planet is inhabited. The order has been canceled.", fleet));
        }

        public static void RemoteMineDeepSpace(Player player, Fleet fleet)
        {
            player.Messages.Add(new Message(MessageType.Invalid,
                $"Remote mining robots from {fleet.Name} had orders to mine in deep space. The order has been canceled.", fleet));
        }

        public static void PlanetDiscovered(Player player, Planet planet, Hab terraformAmount)
        {
            long habValue = player.Race.GetPlanetHabitability(planet.BaseHab.Value);
            string text;
            if (planet.Owned && planet.PlayerNum != player.Num)
            {
                text = $"You have found a planet occupied by someone else. {planet.Name} is currently owned by the {planet.RacePluralName}";
            }
            else
            {
                Hab terraformedHab = planet.Hab.Value + terraformAmount;
                long terraformHabValue = habValue;

                if (planet.BaseHab.Value != terraformedHab)
                {
                    terraformHabValue = player.Race.GetPlanetHabitability(terraformedHab);
                }

                double growth = (habValue / 100.0) * player.Race.GrowthRate;
                if (habValue > 0)
                {
                    text = $"You have found a new habitable planet.  Your colonists will grow by up {growth:0.##}% per year if you colonize {planet.Name}";
                }
                else
                {
                    if (terraformHabValue > 0)
                    {
                        double terraformGrowth = (terraformHabValue / 100.0) * player.Race.GrowthRate;
                        text = $"You have found a new planet which you have the ability to make habitable. With terraforming, your colonists will grow by up to {terraformGrowth:0.##}% per year if you colonize {planet.Name}.";
                    }
                    else
                    {
                        text = $"You have found a new planet which unfortunately is not habitable by you.  {growth:0.##}% of your colonists will die per year if you colonize {planet.Name}";
                    }
                }
            }

            player.Messages.Add(new Message(MessageType.PlanetDiscovery, text, planet));
        }

        public static void FleetReproduce(Player player, Fleet fleet, int colonistsGrown, Planet planet = null, int over = 0)
        {
            string text;
            if (planet == null || over == 0)
            {
                text = $"Your colonists in {fleet.Name} have made good use of their time increasing their on-board number by {colonistsGrown} colonists.";
            }
            else
            {
                text = $"Breeding activities on {fleet.Name} have overflowed living space. {over} colonists have been beamed down to {planet.Name}.";
            }
            player.Messages.Add(new Message(MessageType.FleetReproduce, text, fleet));
        }

        public static void FleetCompletedAssignedOrders(Player player, Fleet fleet)
        {
            string text = $"{fleet.Name} has completed its assigned orders";
            player.Messages.Add(new Message(MessageType.FleetOrdersComplete, text, fleet));
        }

        public static void PlanetBombed(Player player, Planet planet, Fleet fleet, int colonistsKilled, int minesDestroyed, int factoriesDestroyed, int defensesDestroyed)
        {
            string text;

            if (player.Num == fleet.PlayerNum)
            {
                if (planet.Population == 0)
                {
                    text = $"Your {fleet.Name} has bombed {planet.RaceName} {planet.Name} killing off all colonists";
                }
                else
                {
                    text = $"Your {fleet.Name} has bombed {planet.RaceName} {planet.Name} killing {colonistsKilled:n0} colonists, and destroying {minesDestroyed:n0} mines, {factoriesDestroyed:n0} factories, and {defensesDestroyed:n0} defenses.";
                }
                player.Messages.Add(new Message(MessageType.EnemyPlanetBombed, text, planet));
            }
            else
            {
                if (planet.Population == 0)
                {
                    text = $"{fleet.RaceName} {fleet.Name} has bombed your {planet.Name} killing off all colonists";
                }
                else
                {
                    text = $"{fleet.RaceName} {fleet.Name} has bombed your {planet.Name} killing {colonistsKilled:n0} colonists, and destroying {minesDestroyed:n0} mines, {factoriesDestroyed:n0} factories, and {defensesDestroyed:n0} defenses.";
                }

                player.Messages.Add(new Message(MessageType.MyPlanetBombed, text, planet));
            }

        }

        public static void PlanetSmartBombed(Player player, Planet planet, Fleet fleet, int colonistsKilled)
        {
            string text;

            if (player.Num == fleet.PlayerNum)
            {
                if (planet.Population == 0)
                {
                    text = $"Your fleet {fleet.Name} has bombed {planet.RaceName} planet {planet.Name} with smart bombs killing all colonists";

                }
                else
                {
                    text = $"Your {fleet.Name} has bombed {planet.RaceName} planet {planet.Name} with smart bombs killing {colonistsKilled:n0} colonists.";
                }
                player.Messages.Add(new Message(MessageType.EnemyPlanetBombed, text, planet));
            }
            else
            {
                if (planet.Population == 0)
                {
                    text = $"{fleet.RaceName} {fleet.Name} has bombed your {planet.Name} with smart bombs killing all colonists";
                }
                else
                {
                    text = $"{fleet.RaceName} {fleet.Name} has bombed your {planet.Name} with smart bombs killing {colonistsKilled:n0} colonists.";
                }
                player.Messages.Add(new Message(MessageType.MyPlanetBombed, text, planet));
            }
        }

        public static void PlanetRetroBombed(Player player, Planet planet, Fleet fleet, Hab unterraformAmount)
        {
            string text;

            if (player.Num == fleet.PlayerNum)
            {
                text = $"Your fleet {fleet.Name} has retro-bombed {planet.RaceName} planet {planet.Name}, undoing {unterraformAmount.AbsSum}% of its terraforming.";
                player.Messages.Add(new Message(MessageType.EnemyPlanetRetroBombed, text, planet));
            }
            else
            {
                text = $"{fleet.RaceName} {fleet.Name} has retro-bombed your {planet.Name}, undoing {unterraformAmount.AbsSum}% of its terraforming.";
                player.Messages.Add(new Message(MessageType.MyPlanetRetroBombed, text, planet));
            }
        }

        public static void InvadeEmptyPlanet(Player player, Fleet fleet, Planet planet)
        {
            string text = $"{fleet.Name} has attempted to invade {planet.Name}, but the planet is uninhabited.";
            player.Messages.Add(new Message(MessageType.Invalid, text, planet));
        }


        public static void PlanetInvaded(Player player, Planet planet, Fleet fleet, int attackersKilled, int defendersKilled)
        {
            string text;

            if (player.Num == fleet.PlayerNum)
            {
                if (planet.PlayerNum == fleet.PlayerNum)
                {
                    // we invaded and won
                    text = $"Your {fleet.Name} has successfully invaded {planet.RaceName} planet {planet.Name} killing off all colonists";
                }
                else
                {
                    // we invaded and lost
                    text = $"Your {fleet.Name} tried to invade {planet.Name}, but all of your colonists were killed by {planet.RacePluralName}. You valiant fighters managed to kill {defendersKilled:n0} of their colonists.";
                }
                player.Messages.Add(new Message(MessageType.EnemyPlanetInvaded, text, planet));
            }
            else
            {
                if (planet.PlayerNum == fleet.PlayerNum)
                {
                    // we were invaded, and lost
                    text = $"{fleet.RaceName} {fleet.Name} has successfully invaded your planet {planet.Name}, killing off all of your colonists";
                }
                else
                {
                    // we were invaded, and lost
                    text = $"{fleet.RaceName} {fleet.Name} tried to invade {planet.Name}, but you were able to fend them off. You lost {defendersKilled:n0} colonists in the invasion.";
                }
                player.Messages.Add(new Message(MessageType.MyPlanetInvaded, text, planet));
            }

        }

        public static void Battle(Player player, Planet planet, Vector2 position, BattleRecord record)
        {
            string text;
            string location = planet != null ? $"at {planet.Name}" : $"in deep space at {TextUtils.GetPositionString(position)}";

            // TODO: this should be more descriptive like "A battle took place at Zebra against the Cleavers. Neither your 7 forces nor the
            // Cleaver Smaugarian Peeping Tom were completely wiped out. You lost 0."
            text = $"A battle took place {location}";
            player.Messages.Add(new Message(MessageType.Battle, text, planet) { BattleGuid = record.Guid });
        }

        public static void FleetHitMineField(Player player, Fleet fleet, MineField mineField, int damage, int shipsDestroyed)
        {
            string text = "";
            MapObject messageTarget = mineField;
            if (fleet.PlayerNum == player.Num)
            {
                // it's our fleet, it must be someone else's minefield
                if (fleet.Spec.TotalShips <= shipsDestroyed)
                {
                    text = $"{fleet.Name} has been annihilated in a {mineField.Type} mine field at {TextUtils.GetPositionString(mineField.Position)}";
                }
                else
                {
                    messageTarget = fleet;
                    text = $"{fleet.Name} has been stopped in a {mineField.Type} mine field at {TextUtils.GetPositionString(mineField.Position)}.";
                    if (damage > 0)
                    {
                        if (shipsDestroyed > 0)
                        {
                            text += $" Your fleet has taken {damage} damage points and {shipsDestroyed} ships were destroyed.";
                        }
                        else
                        {
                            text += $" Your fleet has taken {damage} damage points but none of your ships were destroyed.";
                        }
                    }
                    else
                    {
                        text = $"{fleet.Name} has been stopped in a {mineField.Type} mine field at {TextUtils.GetPositionString(mineField.Position)}.";
                    }
                }
            }
            else
            {
                // it's not our fleet, it must be our minefield
                if (fleet.Spec.TotalShips <= shipsDestroyed)
                {
                    text = $"{fleet.RaceName} {fleet.Name} has been annihilated in your {mineField.Type} mine field at {TextUtils.GetPositionString(mineField.Position)}";
                }
                else
                {
                    messageTarget = fleet;
                    text = $"{fleet.RaceName} {fleet.Name} has been stopped in your {mineField.Type} mine field at {TextUtils.GetPositionString(mineField.Position)}.";
                    if (damage > 0)
                    {
                        if (shipsDestroyed > 0)
                        {
                            text += $" Your mines have inflicted {damage} damage points and destroyed {shipsDestroyed} ships.";
                        }
                        else
                        {
                            text += $" Your mines have inflicted {damage} damage points but you didn't manage to destroy any ships.";
                        }
                    }
                    else
                    {
                        text = $"{fleet.Name} has been stopped in your {mineField.Type} mine field at {TextUtils.GetPositionString(mineField.Position)}.";
                    }
                }
            }

            player.Messages.Add(new Message(MessageType.MineFieldHit, text, messageTarget));
        }

        public static void MinesLaid(Player player, Fleet fleet, MineField mineField, int numMinesLaid)
        {
            string text = "";
            if (mineField.NumMines == numMinesLaid)
            {
                text = $"{fleet.Name} has dispersed {numMinesLaid} mines.";
            }
            else
            {
                text = $"{fleet.Name} has increased a minefield by {numMinesLaid} mines.";
            }
            player.Messages.Add(new Message(MessageType.MinesLaid, text, fleet));
        }

        public static void MinesLaidFailed(Player player, Fleet fleet)
        {
            var text = $"{fleet.Name} has attempted to lay mines. The order has been cancelled because the fleet has no mine layers.";
            player.Messages.Add(new Message(MessageType.Invalid, text, fleet));
        }

        public static void MineFieldSwept(Player player, Fleet fleet, MineField mineField, long numMinesSwept)
        {
            string text = "";
            MapObject target = mineField;
            if (fleet.PlayerNum == player.Num)
            {
                text = $"{fleet.Name} has swept {numMinesSwept} from a mineField at {TextUtils.GetPositionString(mineField.Position)}";
            }
            else
            {
                text = $"Somone has swept {numMinesSwept} from your mineField at {TextUtils.GetPositionString(mineField.Position)}";
            }
            if (mineField.NumMines <= 10)
            {
                if (fleet.PlayerNum == player.Num)
                {
                    // target the fleet if the mineField is gone
                    target = fleet;
                }
                else
                {
                    target = null;
                }
            }
            player.Messages.Add(new Message(MessageType.MinesSwept, text, target));
        }

        public static void RandomMineralDeposit(Player player, Planet planet, MineralType mineralType)
        {
            var text = $"Your surveyors on {planet.Name} have discovered a previously unknown deposit of {mineralType}, significantly increasing the planet's concentration.";
            player.Messages.Add(new Message(MessageType.RandomMineralDeposit, text, planet));
        }

        public static void Victory(Player player, Player victor)
        {
            string text = "";
            if (player.Num == victor.Num)
            {
                text = $"You have been declared the winner of this game. You may continue to play though, if you wish to really rub everyone's nose in your grand victory.";
            }
            else
            {
                text = $"The forces of {player.Race.PluralName} have been declared the winner of this game. You are advised to accept their supremacy, though you may continue the fight.";
            }
            // victory messages are always the first message of the year
            player.Messages.Insert(0, new Message(MessageType.Victor, text));
        }


    }
}