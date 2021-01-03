using CraigStars.Singletons;
using Godot;
using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class Player : Node
    {
        PackedScene fleetScene;

        public int NetworkId { get; set; }
        public int Num { get; set; }
        public string PlayerName { get; set; }
        public Boolean Ready { get; set; } = false;
        public Boolean AIControlled { get; set; }
        public Color Color { get; set; } = Colors.Black;
        public Race Race = new Race();
        public Planet Homeworld { get; set; }

        #region Scanner Data

        public List<Planet> Planets { get; set; } = new List<Planet>();
        public Dictionary<Guid, Planet> PlanetsByGuid { get; set; } = new Dictionary<Guid, Planet>();
        public List<Fleet> Fleets { get; set; } = new List<Fleet>();
        public List<Message> Messages { get; set; } = new List<Message>();

        #endregion

        # region Research

        /// <summary>
        /// Each player has a certain level of each tech.
        /// </summary>
        /// <returns></returns>
        public TechLevel TechLevels = new TechLevel();

        /// <summary>
        /// The amount spent on each tech level
        /// </summary>
        /// <returns></returns>
        public TechLevel TechLevelsSpent = new TechLevel();

        /// <summary>
        /// The percentage of resources to spend on research
        /// </summary>
        /// <value></value>
        public double ResearchAmount { get; set; } = 15;
        public TechField Researching { get; set; } = TechField.Energy;
        public NextResearchField NextResearchField { get; set; } = NextResearchField.SameField;

        #endregion

        #region Calculated Values

        /// <summary>
        /// The current PlanetaryScanner tech this player has researched
        /// </summary>
        /// <value></value>
        public TechPlanetaryScanner PlanetaryScanner { get; set; }

        #endregion

        public override void _Ready()
        {
            fleetScene = ResourceLoader.Load<PackedScene>("res://src/GameObjects/Fleet.tscn");
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
        public void ResearchNextLevel(UniverseSettings settings, int resourcesToSpend)
        {
            // add the resourcesToSpend to how much we've currently spent
            TechLevelsSpent[Researching] += resourcesToSpend;
            var spentOnCurrentLevel = TechLevelsSpent[Researching];

            // don't research more than the max on this level
            // TODO: If we get to max level, automatically switch to the lowest field
            var level = TechLevels[Researching];
            if (level >= settings.TechBaseCost.Length)
            {
                Message.Info(this, $"You are already at level {level} in {Researching} and cannot research further.");
                return;
            }

            // figure out our total levels
            var totalLevels = TechLevels.Sum();

            // figure out the cost to advance to the next level
            var baseCost = settings.TechBaseCost[level + 1];
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
                    ResearchNextLevel(settings, leftoverResources);
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

            // we made it here, if we have the levels, we have the tech
            return TechLevels.HasRequiredLevels(requirements);
        }

        /// <summary>
        /// Get the best planetary scanner this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechPlanetaryScanner GetBestPlanetaryScanner(TechStore techStore)
        {
            var planetaryScanners = techStore.TechsByCategory[TechCategory.PlanetaryScanner];
            // sort from highest to lowest, return the first match
            planetaryScanners.Sort((t1, t2) => t2.Ranking.CompareTo(t1.Ranking));
            var tech = planetaryScanners.Find(t => HasTech(t));

            return tech as TechPlanetaryScanner;
        }

        /// <summary>
        /// Update our report of this planet
        /// Note: This should never be an RPC call, it should always
        /// be called on the server
        /// </summary>
        /// <param name="planet"></param>
        public void UpdateReport(Planet planet)
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
        /// Update the report for this fleet
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="penScanned">True if we penscanned it</param>
        public void AddFleetReport(Fleet fleet, bool penScanned)
        {
            var fleetReport = fleetScene.Instance() as Fleet;
            fleetReport.Guid = fleet.Guid;
            fleetReport.Position = fleet.Position;
            fleetReport.ObjectName = fleet.ObjectName;
            fleetReport.RaceName = fleet.Player.Race.Name;
            fleetReport.RacePluralName = fleet.Player.Race.PluralName;

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
                        fleetReport.Waypoints.Add(new Waypoint(targetedPlanetReport));
                    }
                    else
                    {
                        // TODO: figure out targeting other fleets
                        // we might have to add waypoint reports as a separate
                        // step
                        var fleetReportWaypoint = new Waypoint();
                        fleetReportWaypoint.Position = wp.Position;
                        fleetReport.Waypoints.Add(fleetReportWaypoint);
                    }
                });
                fleetReport.Cargo.Copy(fleet.Cargo);
                // TODO: Copy ship tokens? We don't want user modified
                // ShipDesigns to impact the server...
                fleetReport.Tokens.AddRange(fleet.Tokens);
            }

            Fleets.Add(fleetReport);
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

    }
}
