using Godot;
using System.Collections.Generic;
using System.Linq;

using CraigStars.Singletons;
using System;

namespace CraigStars
{
    public class Game : Node
    {
        public UniverseSettings UniverseSettings { get; set; } = new UniverseSettings();
        public TechStore TechStore { get => TechStore.Instance; }
        public List<Player> Players { get; set; } = new List<Player>();
        public List<Planet> Planets { get; set; } = new List<Planet>();
        public List<Fleet> Fleets { get; set; } = new List<Fleet>();
        public Dictionary<Guid, Planet> PlanetsByGuid { get; set; } = new Dictionary<Guid, Planet>();
        public Dictionary<Guid, Fleet> FleetsByGuid { get; set; } = new Dictionary<Guid, Fleet>();
        public List<MineralPacket> MineralPackets { get; set; } = new List<MineralPacket>();
        public List<MineField> MineFields { get; set; } = new List<MineField>();
        public int Width { get; set; }
        public int Height { get; set; }
        public int Year { get; set; } = 2400;

        Scanner Scanner { get; set; }

        public override void _Ready()
        {
            Players.AddRange(PlayersManager.Instance.Players);

            // generate a new univers
            UniverseGenerator generator = new UniverseGenerator();
            generator.Generate(this, UniverseSettings, PlayersManager.Instance.Players);


            // update our player information as if we'd just generated a new turn
            TurnGenerator turnGenerator = new TurnGenerator();
            turnGenerator.UpdatePlayerReports(this, TechStore);

            // add the universe to the viewport
            Scanner = FindNode("Scanner") as Scanner;
            Scanner.AddMapObjects(PlayersManager.Instance.Me);

            Signals.PublishPostStartGameEvent(Year);

            Signals.SubmitTurnEvent += OnSubmitTurn;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("submit_turn"))
            {
                // submit our turn
                Signals.PublishSubmitTurnEvent(PlayersManager.Instance.Me);
            }
            if (@event.IsActionPressed("technology_browser"))
            {
                GetTree().ChangeScene("res://src/GUI/ShipDesigner/HullSummary.tscn");
            }
        }

        void UpdateDictionaries()
        {
            // build each players dictionary of planets by id
            PlanetsByGuid = Planets.ToLookup(p => p.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);
            FleetsByGuid = Fleets.ToLookup(p => p.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);
        }

        /// <summary>
        /// The player has submitted a new turn.
        /// Copy any data from this to the main game
        /// </summary>
        /// <param name="player"></param>
        void OnSubmitTurn(Player player)
        {
            // TODO: fix this to have turn submit data
            foreach (var playerFleet in player.Fleets.Where(f => f.Player == player))
            {
                if (FleetsByGuid.TryGetValue(playerFleet.Guid, out var fleet) && fleet.Player == player)
                {
                    // replace each waypoint operation with whatever we got from the client
                    fleet.Waypoints.RemoveRange(1, fleet.Waypoints.Count - 1);
                    if (playerFleet.Waypoints != null && playerFleet.Waypoints.Count > 0)
                    {
                        // TODO: copy the task as well when we implement it
                        fleet.Waypoints[0].WarpFactor = playerFleet.Waypoints[0].WarpFactor;
                        foreach (var playerWaypoint in playerFleet.Waypoints.Skip(1))
                        {
                            if (playerWaypoint.Target is Planet planet)
                            {
                                if (PlanetsByGuid.TryGetValue(planet.Guid, out var gamePlanet))
                                {
                                    // add the server side version of this planet as a waypoint
                                    fleet.Waypoints.Add(new Waypoint(gamePlanet));
                                }
                            }
                        };
                    }
                }
            }

            // TODO: support multiple players, production queues, etc
            GenerateTurn();
        }


        void GenerateTurn()
        {
            TurnGenerator generator = new TurnGenerator();
            generator.GenerateTurn(this, TechStore);
            generator.UpdatePlayerReports(this, TechStore);
            UpdateDictionaries();
            Signals.PublishTurnPassedEvent(Year);
        }
    }
}
