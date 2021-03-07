using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    public class Player : PublicPlayerInfo
    {
        static ILog log = LogManager.GetLogger(typeof(Player));

        /// <summary>
        /// Each player gets a copy of rules from the server. These rules are used
        /// for computing various values both for the UI and during turn generation
        /// </summary>
        public Rules Rules { get; set; } = new Rules();

        /// <summary>
        /// Each player gets a TechStore from the server (or client)
        /// Note: this defaults to StaticTechStore for testing
        /// </summary>
        [JsonIgnore]
        public ITechStore TechStore { get; set; } = StaticTechStore.Instance;

        public Race Race { get; set; } = new Race();
        public PlayerStats Stats { get; set; } = new PlayerStats();

        #region Research

        /// <summary>
        /// Each player has a certain level of each tech.
        /// </summary>
        /// <returns></returns>
        public TechLevel TechLevels { get; set; } = new TechLevel();

        /// <summary>
        /// The amount spent on each tech level
        /// </summary>
        /// <returns></returns>
        public TechLevel TechLevelsSpent { get; set; } = new TechLevel();

        /// <summary>
        /// The percentage of resources to spend on research
        /// </summary>
        /// <value></value>
        public int ResearchAmount { get; set; } = 15;
        public TechField Researching { get; set; } = TechField.Energy;
        public NextResearchField NextResearchField { get; set; } = NextResearchField.LowestField;

        #endregion

        #region Turn Actions

        public List<CargoTransferOrder> CargoTransferOrders { get; set; } = new List<CargoTransferOrder>();

        #endregion

        #region Designs

        [JsonProperty(ItemIsReference = true)]
        public List<ShipDesign> Designs { get; set; } = new List<ShipDesign>();

        [JsonProperty(ItemIsReference = true)]
        public List<ShipDesign> DeletedDesigns { get; set; } = new List<ShipDesign>();

        #endregion

        #region Universe Data
        public int Year { get; set; } = 2400;

        public PlanetViewState PlanetViewState { get; set; }

        [JsonProperty(ItemIsReference = true)]
        public List<Planet> Planets { get; set; } = new List<Planet>();

        [JsonProperty(ItemIsReference = true)]
        public List<Fleet> Fleets { get; set; } = new List<Fleet>();

        [JsonProperty(ItemIsReference = true)]
        public List<Fleet> DeletedFleets { get; set; } = new List<Fleet>();

        [JsonProperty(ItemIsReference = true)]
        public List<Message> Messages { get; set; } = new List<Message>();

        [JsonProperty(IsReference = true)]
        public Planet Homeworld { get; set; }

        [JsonIgnore]
        public Dictionary<Guid, ShipDesign> DesignsByGuid { get; set; } = new Dictionary<Guid, ShipDesign>();

        [JsonIgnore]
        public Dictionary<Guid, Planet> PlanetsByGuid { get; set; } = new Dictionary<Guid, Planet>();

        [JsonIgnore]
        public Dictionary<Guid, Fleet> FleetsByGuid { get; set; } = new Dictionary<Guid, Fleet>();

        #endregion

        #region Calculated Values

        /// <summary>
        /// The current PlanetaryScanner tech this player has researched
        /// </summary>
        /// <value></value>
        public TechPlanetaryScanner PlanetaryScanner { get; set; }

        #endregion

        #region Serializer Helpers

        public void SetupMapObjectMappings()
        {
            DesignsByGuid = Designs.ToLookup(d => d.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);
            PlanetsByGuid = Planets.ToLookup(p => p.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);
            FleetsByGuid = Fleets.ToLookup(f => f.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);
        }

        public void ComputeAggregates()
        {
            Designs.ForEach(d => d.ComputeAggregate(this));
            Fleets.ForEach(f => f.ComputeAggregate());
            Planets.ForEach(p => p.Starbase?.ComputeAggregate());
        }

        #endregion

        /// <summary>
        /// Delete this design and also delete any tokens that use the design
        /// </summary>
        /// <param name="design"></param>
        public void DeleteDesign(ShipDesign design)
        {
            var designIndex = Designs.FindIndex(d => d == design);

            DeletedDesigns.Add(Designs[designIndex]);
            Designs.RemoveAt(designIndex);

            foreach (var fleet in Fleets.Where(f => f.OwnedBy(this)))
            {
                fleet.Tokens = fleet.Tokens.Where(token => token.Design != design).ToList();
            }
        }

        /// <summary>
        /// This function will be called recursively until no more levels are passed
        /// From starsfaq
        ///   The cost of a tech level depends on four things: 
        ///  1) Your research setting for that field (cheap, normal, or expensive) 
        ///  2) The level you are researching (higher level, higher cost) 
        ///  3) The total number of tech levels you have already have in all fields (you can add it up yourself, or look at 'tech levels' on the 'score' screen). 
        ///  4) whether 'slow tech advance' was selected as a game parameter.
        ///
        ///  in general,
        ///
        ///  totalCost=(baseCost + (totalLevels * 10)) * costFactor
        ///
        ///  where  totalLevels=the sum of your current levels in all fields 
        ///    costFactor =.5 if your setting for the field is '50% less' 
        ///                       =1 if your setting for the field is 'normal' 
        ///                       =1.75 if your setting for the field is '75% more expensive'
        ///
        //  If 'slow tech advance' is a game parameter, totalCost should be doubled.
        ///
        ///  Below is a table showing the base cost of each level. 
        ///
        ///  1     50              14    18040 
        ///  2     80              15    22440 
        ///  3     130             16    27050 
        ///  4     210             17    31870 
        ///  5     340             18    36900 
        ///  6     550             19    42140 
        ///  7     890             20    47590 
        ///  8     1440            21    53250 
        ///  9     2330            22    59120 
        ///  10    3770            23    65200 
        ///  11    6100            24    71490 
        ///  12    9870            25    77990 
        ///  13    13850           26    84700
        /// 
        /// 
        /// </summary>
        /// <param name="resourcesToSpend">The amount of resources to spend on research</param>
        public void ResearchNextLevel(int resourcesToSpend)
        {
            // add the resourcesToSpend to how much we've currently spent
            TechLevelsSpent[Researching] += resourcesToSpend;
            var spentOnCurrentLevel = TechLevelsSpent[Researching];

            // don't research more than the max on this level
            // TODO: If we get to max level, automatically switch to the lowest field
            var level = TechLevels[Researching];
            if (level >= Rules.TechBaseCost.Length - 1)
            {
                Message.Info(this, $"You are already at level {level} in {Researching} and cannot research further.");
                return;
            }

            // figure out our total levels
            var totalLevels = TechLevels.Sum();

            // figure out the cost to advance to the next level
            var baseCost = Rules.TechBaseCost[level + 1];
            var researchCost = Race.ResearchCost[Researching];
            var costFactor = 1f;
            switch (researchCost)
            {
                case ResearchCostLevel.Extra:
                    costFactor = 1.75f;
                    break;
                case ResearchCostLevel.Less:
                    costFactor = .5f;
                    break;
            }

            // from starsfaq
            int totalCost = (int)((baseCost + (totalLevels * 10)) * costFactor);

            if (spentOnCurrentLevel >= totalCost)
            {
                // increase a level
                TechLevels[Researching]++;

                // figure out how many leftover points we have
                var leftoverResources = spentOnCurrentLevel - totalCost;

                // reset the amount we spent to zero
                TechLevelsSpent[Researching] = 0;

                // determine the next field to research
                var nextField = GetNextResearchField();

                // notify our player that we got a new level
                Message.TechLevel(this, Researching, TechLevels[Researching], nextField);

                // setup the next level
                Researching = nextField;

                if (leftoverResources > 0)
                {
                    // we have leftover resources, so call ourselves again
                    // to apply them to the next level
                    ResearchNextLevel(leftoverResources);
                }
            }
        }

        /// <summary>
        /// Get the next TechField to research based on the NextResearchField setting
        /// </summary>
        /// <returns></returns>
        public TechField GetNextResearchField()
        {
            var nextField = Researching;
            switch (NextResearchField)
            {
                case NextResearchField.Energy:
                    nextField = TechField.Energy;
                    break;
                case NextResearchField.Weapons:
                    nextField = TechField.Weapons;
                    break;
                case NextResearchField.Propulsion:
                    nextField = TechField.Propulsion;
                    break;
                case NextResearchField.Construction:
                    nextField = TechField.Construction;
                    break;
                case NextResearchField.Electronics:
                    nextField = TechField.Electronics;
                    break;
                case NextResearchField.Biotechnology:
                    nextField = TechField.Biotechnology;
                    break;
                case NextResearchField.LowestField:
                    nextField = TechLevels.Lowest();
                    break;
            }

            return nextField;
        }

        /// <summary>
        /// Returns true if the player has this tech
        /// </summary>
        /// <param name="tech">The tech to check requirements for</param>
        /// <returns>True if this player has access to this tech</returns>
        public bool HasTech(Tech tech)
        {
            // we made it here, if we have the levels, we have the tech
            return CanLearnTech(tech) && TechLevels.HasRequiredLevels(tech.Requirements);
        }

        /// <summary>
        /// Can the player ever learn this tech?
        /// </summary>
        /// <param name="tech"></param>
        /// <returns></returns>
        public bool CanLearnTech(Tech tech)
        {
            TechRequirements requirements = tech.Requirements;
            if (requirements.PRTRequired != PRT.None && requirements.PRTRequired != Race.PRT)
            {
                return false;
            }
            if (requirements.PRTDenied != PRT.None && Race.PRT == requirements.PRTDenied)
            {
                return false;
            }

            foreach (LRT lrt in requirements.LRTsRequired)
            {
                if (!Race.HasLRT(lrt))
                {
                    return false;
                }
            }

            foreach (LRT lrt in requirements.LRTsDenied)
            {
                if (Race.HasLRT(lrt))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get the best planetary scanner this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechPlanetaryScanner GetBestPlanetaryScanner()
        {
            return GetBestTech<TechPlanetaryScanner>(TechStore, TechCategory.PlanetaryScanner);
        }

        /// <summary>
        /// Get the best beam weapon this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechDefense GetBestDefense()
        {
            return GetBestTech<TechDefense>(TechStore, TechCategory.PlanetaryDefense);
        }

        /// <summary>
        /// Get the best engine this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechEngine GetBestEngine()
        {
            return GetBestTech<TechEngine>(TechStore, TechCategory.Engine);
        }

        /// <summary>
        /// Get the best shield this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechHullComponent GetBestScanner()
        {
            return GetBestTech<TechHullComponent>(TechStore, TechCategory.Scanner);
        }

        /// <summary>
        /// Get the best shield this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechHullComponent GetBestShield()
        {
            return GetBestTech<TechHullComponent>(TechStore, TechCategory.Shield);
        }

        /// <summary>
        /// Get the best armor this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechHullComponent GetBestArmor()
        {
            return GetBestTech<TechHullComponent>(TechStore, TechCategory.Armor);
        }

        /// <summary>
        /// Get the best beam weapon this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechHullComponent GetBestBeamWeapon()
        {
            return GetBestTech<TechHullComponent>(TechStore, TechCategory.BeamWeapon);
        }

        /// <summary>
        /// Get the best torpedo this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechHullComponent GetBestTorpedo()
        {
            return GetBestTech<TechHullComponent>(TechStore, TechCategory.Torpedo);
        }

        /// <summary>
        /// Get the best tech by category
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="techStore"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public T GetBestTech<T>(ITechStore techStore, TechCategory category) where T : Tech
        {
            var techs = techStore.GetTechsByCategory(category);
            techs.Sort((t1, t2) => t2.Ranking.CompareTo(t1.Ranking));
            var tech = techs.Find(t => HasTech(t));
            return tech as T;
        }

        public ShipDesign GetDesign(string name)
        {
            return Designs.Find(d => d.Name == name);
        }

        /// <summary>
        /// Update the player with design information
        /// </summary>
        /// <param name="design"></param>
        /// <param name="penScanned"></param>
        public void UpdateDesignReport(ShipDesign design, bool penScanned)
        {
            if (DesignsByGuid.TryGetValue(design.Guid, out var playerDesign))
            {
                playerDesign.Name = design.Name;
                playerDesign.Owner = design.Owner;
                playerDesign.Hull = design.Hull;
                playerDesign.HullSetNumber = design.HullSetNumber;
                if (penScanned && playerDesign.Slots.Count == 0)
                {
                    // copy all the slot data so our player knows about it
                    playerDesign.Slots.AddRange(design.Slots.Select(slot => new ShipDesignSlot(slot.HullComponent, slot.HullSlotIndex, slot.Quantity)));
                }
            }
            else
            {
                var newDesign = design.Clone();
                Designs.Add(newDesign);
                DesignsByGuid[newDesign.Guid] = newDesign;
                if (!penScanned)
                {
                    // don't show the player slots if it was scanned with penetrating scanners
                    newDesign.Slots.Clear();
                }
            }
        }

        /// <summary>
        /// Update our report of this planet
        /// Note: This should never be an RPC call, it should always
        /// be called on the server
        /// </summary>
        /// <param name="planet"></param>
        public void UpdatePlanetReport(Planet planet)
        {
            var planetReport = PlanetsByGuid[planet.Guid];
            if (planetReport.ReportAge > 0)
            {
                planetReport.UpdatePlanetReport(planet);
            }
            else if (planetReport.ReportAge == Planet.Unexplored)
            {
                planetReport.UpdatePlanetReport(planet);
                Message.PlanetDiscovered(this, planetReport);
            }
        }

        /// <summary>
        /// Called by the server to update the player's copy
        /// of their own planets. This will copy mines, factories, production queues
        /// etc
        /// </summary>
        /// <param name="planet"></param>
        public void UpdatePlayerPlanet(Planet planet)
        {
            var planetReport = PlanetsByGuid[planet.Guid];
            planetReport.Player = this;
            planetReport.UpdatePlayerPlanet(planet);
        }

        /// <summary>
        /// Update the report for this fleet
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="penScanned">True if we penscanned it</param>
        public void AddFleetReport(Fleet fleet, bool penScanned)
        {
            var fleetReport = new Fleet()
            {
                Guid = fleet.Guid,
                Position = fleet.Position,
                Name = fleet.Name,
                RaceName = fleet.Player.Race.Name,
                RacePluralName = fleet.Player.Race.PluralName,
            };

            // update orbiting information
            if (fleet.Orbiting != null)
            {
                var orbitingPlanetReport = PlanetsByGuid[fleet.Orbiting.Guid];
                fleetReport.Orbiting = orbitingPlanetReport;
                fleetReport.Orbiting.OrbitingFleets.Add(fleetReport);
            }

            if (this == fleet.Player)
            {
                fleetReport.Player = this;
                fleetReport.Waypoints.Clear();
                // copy waypoints
                fleet.Waypoints.ForEach(wp =>
                {
                    if (wp.Target is Planet)
                    {
                        var targetedPlanetReport = PlanetsByGuid[wp.Target.Guid];
                        fleetReport.Waypoints.Add(Waypoint.TargetWaypoint(targetedPlanetReport, wp.WarpFactor, wp.Task, wp.TransportTasks));
                    }
                    else
                    {
                        // TODO: figure out targeting other fleets
                        // we might have to add waypoint reports as a separate
                        // step
                        fleetReport.Waypoints.Add(Waypoint.PositionWaypoint(wp.Position, wp.WarpFactor, wp.Task, wp.TransportTasks));
                    }
                });
                fleetReport.Cargo = fleet.Cargo;
                // TODO: Copy ship tokens? We don't want user modified
                // ShipDesigns to impact the server...
                fleetReport.Tokens.AddRange(fleet.Tokens);

                foreach (var token in fleetReport.Tokens)
                {
                    token.Design = DesignsByGuid[token.Design.Guid];
                }
            }

            Fleets.Add(fleetReport);
            FleetsByGuid[fleet.Guid] = fleetReport;
        }

        /// <summary>
        /// Go through each message and update the target to a value from our reports
        /// </summary>
        public void UpdateMessageTargets()
        {
            foreach (var message in Messages)
            {
                if (message.Target != null)
                {
                    if (message.Target is Fleet)
                    {
                        if (FleetsByGuid.TryGetValue(message.Target.Guid, out var fleet))
                        {
                            message.Target = fleet;
                        }
                        else
                        {
                            log.Error($"Found a Message Target with a fleet the player hasn't discovered: {message.Target.Name}");
                        }
                    }
                    else if (message.Target is Planet)
                    {
                        message.Target = PlanetsByGuid[message.Target.Guid];
                    }
                    else
                    {
                        log.Error("Found a Message Target with an unknown type, setting to null.");
                        message.Target = null;
                    }
                }
            }
        }


    }
}
