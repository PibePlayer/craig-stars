using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CraigStars
{
    public class Message
    {
        public MessageType Type { get; set; }
        public String Text { get; set; }

        [JsonProperty(IsReference = true)]
        public MapObject Target { get; set; }

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

        public static void Info(Player player, String text)
        {
            player.Messages.Add(new Message(MessageType.Info, text));
        }

        public static void HomePlanet(Player player, Planet planet)
        {
            String text = $"Your home planet is {planet.Name}.  Your people are ready to leave the nest and explore the universe.  Good luck.";
            player.Messages.Add(new Message(MessageType.HomePlanet, text, player.PlanetsByGuid[planet.Guid]));

        }

        public static void Mine(Player player, Planet planet, int numMines)
        {
            String text = $"You have built {numMines} mine(s) on {planet.Name}.";
            player.Messages.Add(new Message(MessageType.BuiltMine, text, player.PlanetsByGuid[planet.Guid]));

        }

        public static void Factory(Player player, Planet planet, int numFactories)
        {
            String text = $"You have built {numFactories} factory(s) on {planet.Name}.";
            player.Messages.Add(new Message(MessageType.BuiltFactory, text, player.PlanetsByGuid[planet.Guid]));

        }

        public static void Defense(Player player, Planet planet, int numDefenses)
        {
            String text = $"You have built {numDefenses} defense(s) on {planet.Name}.";
            player.Messages.Add(new Message(MessageType.BuiltDefense, text, player.PlanetsByGuid[planet.Guid]));

        }

        public static void TechLevel(Player player, TechField field, int level, TechField nextField)
        {
            String text = $"Your scientists have completed research into Tech Level {level} for {field}.  They will continue their efforts in the {nextField} field.";
            player.Messages.Add(new Message(MessageType.GainTechLevel, text));
        }

        public static void ColonizeNonPlanet(Player player, Fleet fleet)
        {
            String text = $"{fleet.Name} has attempted to colonize a waypoint with no Planet.";
            player.Messages.Add(new Message(MessageType.ColonizeNonPlanet, text));

        }

        public static void ColonizeOwnedPlanet(Player player, Fleet fleet)
        {
            String text = $"{fleet.Name} has attempted to colonize a planet that is already inhabited.";
            player.Messages.Add(new Message(MessageType.ColonizeOwnedPlanet, text));

        }

        public static void ColonizeWithNoModule(Player player, Fleet fleet)
        {
            String text = $"{fleet.Name} has attempted to colonize a planet without a colonization module.";
            player.Messages.Add(new Message(MessageType.ColonizeWithNoColonizationModule, text));

        }

        public static void ColonizeWithNoColonists(Player player, Fleet fleet)
        {
            String text = $"{fleet.Name} has attempted to colonize a planet without bringing any colonists.";
            player.Messages.Add(new Message(MessageType.ColonizeWithNoColonists, text));
        }

        public static void planetColonized(Player player, Planet planet)
        {
            String text = $"Your colonists are now in control of {planet.Name}";
            player.Messages.Add(new Message(MessageType.PlanetColonized, text, player.PlanetsByGuid[planet.Guid]));
        }

        public static void FleetOutOfFuel(Player player, Fleet fleet)
        {
            String text = $"{fleet.Name} has run out of fuel. The fleet's speed has been decreased to Warp 1.";
            player.Messages.Add(new Message(MessageType.FleetOutOfFuel, text, fleet));
        }

        public static void fleetScrapped(Player player, Fleet fleet, int num_minerals, Planet planet)
        {
            String text = $"{fleet.Name} has been dismantled for {num_minerals}kT of minerals which have been deposited on {planet.Name}.";
            player.Messages.Add(new Message(MessageType.FleetScrapped, text, player.PlanetsByGuid[planet.Guid]));
        }

        public static void FleetBuilt(Player player, ShipDesign design, Fleet fleet, int numBuilt)
        {
            String text;
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

        public static void PlanetDiscovered(Player player, Planet planet)
        {
            long habValue = player.Race.GetPlanetHabitability(planet.Hab.Value);
            String text;
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

            player.Messages.Add(new Message(MessageType.PlanetDiscovery, text, player.PlanetsByGuid[planet.Guid]));
        }

        public static void FleetCompletedAssignedOrders(Player player, Fleet fleet)
        {
            String text = $"{fleet.Name} has completed its assigned orders";
            player.Messages.Add(new Message(MessageType.FleetOrdersComplete, text, fleet));
        }

        public static void PlanetBombed(Player player, Planet planet, Fleet fleet, int colonistsKilled, int minesDestroyed, int factoriesDestroyed, int defensesDestroyed)
        {
            string text;

            if (player == fleet.Player)
            {
                text = planet.Population == 0 ? $"Your {fleet.Name} has bombed {planet.Name} killing off all colonists" : $"Your {fleet.Name} has bombed {planet.Name} killing {colonistsKilled} colonists, and destroying {minesDestroyed} mines, {factoriesDestroyed} factories, and {defensesDestroyed} defenses.";
                player.Messages.Add(new Message(MessageType.EnemyPlanetBombed, text, planet));
            }
            else
            {
                text = planet.Population == 0 ? $"{fleet.Name} has bombed your {planet.Name} killing off all colonists" : $"{fleet.Name} has bombed your {planet.Name} killing {colonistsKilled} colonists, and destroying {minesDestroyed} mines, {factoriesDestroyed} factories, and {defensesDestroyed} defenses.";
                player.Messages.Add(new Message(MessageType.MyPlanetBombed, text, planet));
            }

        }

        public static void PlanetSmartBombed(Player player, Planet planet, Fleet fleet, int colonistsKilled)
        {
            string text;

            if (player == fleet.Player)
            {
                text = planet.Population == 0 ? $"Your fleet {fleet.Name} has bombed {planet.Name} with smart bombs killing all colonists" : $"Your {fleet.Name} has bombed {planet.Name} with smart bombs killing {colonistsKilled} colonists.";
                player.Messages.Add(new Message(MessageType.EnemyPlanetBombed, text, planet));
            }
            else
            {
                text = planet.Population == 0 ? $"{fleet.Name} has bombed your {planet.Name} with smart bombs killing all colonists" : $"{fleet.Name} has bombed your {planet.Name} with smart bombs killing {colonistsKilled} colonists.";
                player.Messages.Add(new Message(MessageType.MyPlanetBombed, text, planet));
            }
        }

    }
}