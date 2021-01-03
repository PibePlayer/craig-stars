using System;

namespace CraigStars
{
    public class Message
    {
        public MessageType Type { get; set; }
        public String Text { get; set; }
        public MapObject Target { get; set; }

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
            String text = $"Your home planet is {planet.ObjectName}.  Your people are ready to leave the nest and explore the universe.  Good luck.";
            player.Messages.Add(new Message(MessageType.HomePlanet, text, planet));

        }

        public static void Mine(Player player, Planet planet, int numMines)
        {
            String text = $"You have built {numMines} mine(s) on {planet.ObjectName}.";
            player.Messages.Add(new Message(MessageType.BuiltMine, text, planet));

        }

        public static void Factory(Player player, Planet planet, int numFactories)
        {
            String text = $"You have built {numFactories} factory(s) on {planet.ObjectName}.";
            player.Messages.Add(new Message(MessageType.BuiltFactory, text, planet));

        }

        public static void Defense(Player player, Planet planet, int numDefenses)
        {
            String text = $"You have built {numDefenses} defense(s) on {planet.ObjectName}.";
            player.Messages.Add(new Message(MessageType.BuiltDefense, text, planet));

        }

        public static void TechLevel(Player player, TechField field, int level, TechField nextField)
        {
            String text = $"Your scientists have completed research into Tech Level {level} for {field}.  They will continue their efforts in the {nextField} field.";
            player.Messages.Add(new Message(MessageType.GainTechLevel, text));
        }

        public static void colonizeNonPlanet(Player player, Fleet fleet)
        {
            String text = $"{fleet.ObjectName} has attempted to colonize a waypoint with no Planet.";
            player.Messages.Add(new Message(MessageType.ColonizeNonPlanet, text));

        }

        public static void colonizeOwnedPlanet(Player player, Fleet fleet)
        {
            String text = $"{fleet.ObjectName} has attempted to colonize a planet that is already inhabited.";
            player.Messages.Add(new Message(MessageType.ColonizeOwnedPlanet, text));

        }

        public static void colonizeWithNoModule(Player player, Fleet fleet)
        {
            String text = $"{fleet.ObjectName} has attempted to colonize a planet without a colonization module.";
            player.Messages.Add(new Message(MessageType.ColonizeWithNoColonizationModule, text));

        }

        public static void colonizeWithNoColonists(Player player, Fleet fleet)
        {
            String text = $"{fleet.ObjectName} has attempted to colonize a planet without bringing any colonists.";
            player.Messages.Add(new Message(MessageType.ColonizeWithNoColonists, text));
        }

        public static void planetColonized(Player player, Planet planet)
        {
            String text = $"Your colonists are now in control of {planet.ObjectName}";
            player.Messages.Add(new Message(MessageType.PlanetColonized, text, planet));
        }

        public static void FleetOutOfFuel(Player player, Fleet fleet)
        {
            String text = $"{fleet.ObjectName} has run out of fuel. The fleet's speed has been decreased to Warp 1.";
            player.Messages.Add(new Message(MessageType.FleetOutOfFuel, text, fleet));
        }

        public static void fleetScrapped(Player player, Fleet fleet, int num_minerals, Planet planet)
        {
            String text = $"{fleet.ObjectName} has been dismantled for {num_minerals}kT of minerals which have been deposited on {planet.ObjectName}.";
            player.Messages.Add(new Message(MessageType.FleetScrapped, text, planet));
        }

        public static void PlanetDiscovered(Player player, Planet planet)
        {
            long habValue = player.Race.GetPlanetHabitability(planet.Hab);
            String text;
            if (planet.Player != null && planet.Player != player)
            {
                text = $"You have found a planet occupied by someone else. {planet.ObjectName} is currently owned by the {planet.Player.Race.PluralName}";
            }
            else
            {
                double growth = (habValue / 100.0) * player.Race.GrowthRate;
                if (habValue > 0)
                {
                    text = $"You have found a new habitable planet.  Your colonists will grow by up {growth:0.##}% per year if you colonize {planet.ObjectName}";
                }
                else
                {
                    text = $"You have found a new planet which unfortunately is not habitable by you.  {growth:0.##}% of your colonists will die per year if you colonize {planet.ObjectName}";
                }
            }

            player.Messages.Add(new Message(MessageType.PlanetDiscovery, text, planet));
        }

        public static void fleetCompletedAssignedOrders(Player player, Fleet fleet)
        {
            String text = $"{fleet.ObjectName} has completed its assigned orders";
            player.Messages.Add(new Message(MessageType.FleetOrdersComplete, text, fleet));
        }

    }
}