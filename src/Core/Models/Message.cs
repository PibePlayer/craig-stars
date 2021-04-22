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

        [JsonProperty(IsReference = true)]
        public MapObject Target { get; set; }

        public Guid? BattleGuid { get; set; }

        public Message() { }

        public Message(MessageType type, string text, MapObject target)
        {
            Type = type;
            Text = text;
            Target = target;
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
            string text = $"Your home planet is {planet.Name}.  Your people are ready to leave the nest and explore the universe.  Good luck.";
            player.Messages.Add(new Message(MessageType.HomePlanet, text, planet));

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

        public static void fleetScrapped(Player player, Fleet fleet, int num_minerals, Planet planet)
        {
            string text = $"{fleet.Name} has been dismantled for {num_minerals}kT of minerals which have been deposited on {planet.Name}.";
            player.Messages.Add(new Message(MessageType.FleetScrapped, text, planet));
        }

        public static void FleetBuilt(Player player, ShipDesign design, Fleet fleet, int numBuilt)
        {
            string text;
            if (numBuilt == 1)
            {
                text = $"Your starbase at {fleet.Orbiting.Name} has built a new {design.Name}.";
            }
            else
            {
                text = $"Your starbase at {fleet.Orbiting.Name} has built {numBuilt} new {design.Name}s.";
            }
            player.Messages.Add(new Message(MessageType.BuiltShip, text, fleet));
        }

        public static void FleetTransferred(Player player, Fleet fleet, CargoType cargoType, ICargoHolder cargoTarget, int transferAmount)
        {
            string text = "";
            if (cargoType == CargoType.Colonists)
            {
                if (transferAmount < 0)
                {
                    text = $"{fleet.Name} has beamed {-transferAmount * 100} {cargoType} from {cargoTarget.Name}";
                }
                else
                {
                    text = $"{fleet.Name} has beamed {transferAmount * 100} {cargoType} to {cargoTarget.Name}";
                }
            }
            else
            {
                if (transferAmount < 0)
                {
                    text = $"{fleet.Name} has unloaded {-transferAmount} {cargoType} from {cargoTarget.Name}";
                }
                else
                {
                    text = $"{fleet.Name} has loaded {transferAmount} {cargoType} from {cargoTarget.Name}";
                }
            }
            player.Messages.Add(new Message(MessageType.CargoTransferred, text, fleet));
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

        public static void PlanetDiscovered(Player player, Planet planet)
        {
            long habValue = player.Race.GetPlanetHabitability(planet.Hab.Value);
            string text;
            if (planet.Owner != null && planet.Owner != player)
            {
                text = $"You have found a planet occupied by someone else. {planet.Name} is currently owned by the {planet.Owner.RacePluralName}";
            }
            else
            {
                double growth = (habValue / 100.0) * player.Race.GrowthRate;
                if (habValue > 0)
                {
                    text = $"You have found a new habitable planet.  Your colonists will grow by up {growth:0.##}% per year if you colonize {planet.Name}";
                }
                else
                {
                    text = $"You have found a new planet which unfortunately is not habitable by you.  {growth:0.##}% of your colonists will die per year if you colonize {planet.Name}";
                }
            }

            player.Messages.Add(new Message(MessageType.PlanetDiscovery, text, planet));
        }

        public static void FleetCompletedAssignedOrders(Player player, Fleet fleet)
        {
            string text = $"{fleet.Name} has completed its assigned orders";
            player.Messages.Add(new Message(MessageType.FleetOrdersComplete, text, fleet));
        }

        public static void PlanetBombed(Player player, Planet planet, Fleet fleet, int colonistsKilled, int minesDestroyed, int factoriesDestroyed, int defensesDestroyed)
        {
            string text;

            if (player == fleet.Player)
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

            if (player == fleet.Player)
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

        public static void InvadeEmptyPlanet(Player player, Fleet fleet, Planet planet)
        {
            string text = $"{fleet.Name} has attempted to invade {planet.Name}, but the planet is uninhabited.";
            player.Messages.Add(new Message(MessageType.Invalid, text, planet));
        }


        public static void PlanetInvaded(Player player, Planet planet, Fleet fleet, int attackersKilled, int defendersKilled)
        {
            string text;

            if (player == fleet.Player)
            {
                if (planet.Player == fleet.Player)
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
                if (planet.Player == fleet.Player)
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
            if (fleet.Player == player)
            {
                // it's our fleet, it must be someone else's minefield
                if (fleet.Aggregate.TotalShips <= shipsDestroyed)
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
                if (fleet.Aggregate.TotalShips <= shipsDestroyed)
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
            if (fleet.Player == player)
            {
                text = $"{fleet.Name} has swept {numMinesSwept} from a mineField at {TextUtils.GetPositionString(mineField.Position)}";
            }
            else
            {
                text = $"Somone has swept {numMinesSwept} from your mineField at {TextUtils.GetPositionString(mineField.Position)}";
            }
            if (mineField.NumMines <= 10)
            {
                if (fleet.Player == player)
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
    }
}