using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;

namespace CraigStars.Client
{
    public class TechSummary : Control
    {
        [Inject] protected PlayerTechService playerTechService;

        protected Player Me { get => PlayersManager.Me; }
        protected PublicGameInfo GameInfo { get => PlayersManager.GameInfo; }

        public Tech Tech
        {
            get => tech;
            set
            {
                tech = value;
                UpdateControls();
            }
        }
        Tech tech;

        Label nameLabel;
        TextureRect iconTextureRect;
        CostGrid costGrid;

        Label massLabel;
        Label massAmountLabel;
        Label unavailableLabel;
        Label researchCostLabel;

        Label noneLabel;
        Label spacerLabel;
        Label energyLabel;
        Label weaponsLabel;
        Label propulsionLabel;
        Label constructionLabel;
        Label electronicsLabel;
        Label biotechnologyLabel;

        Label energyReqLabel;
        Label weaponsReqLabel;
        Label propulsionReqLabel;
        Label constructionReqLabel;
        Label electronicsReqLabel;
        Label biotechnologyReqLabel;

        Label labelTraitRequirement;

        Container statsContainer;
        Container descriptionContainer;

        EngineGraph engineGraph;
        DefenseGraph defenseGraph;

        public override void _Ready()
        {
            this.ResolveDependencies();
            base._Ready();
            costGrid = FindNode("CostGrid") as CostGrid;
            nameLabel = FindNode("NameLabel") as Label;
            iconTextureRect = FindNode("IconTextureRect") as TextureRect;

            massLabel = FindNode("MassLabel") as Label;
            massAmountLabel = FindNode("MassAmountLabel") as Label;
            researchCostLabel = FindNode("ResearchCostLabel") as Label;

            // these fields describe the tech requirements
            noneLabel = FindNode("NoneLabel") as Label;
            spacerLabel = FindNode("SpacerLabel") as Label;
            energyLabel = FindNode("EnergyLabel") as Label;
            weaponsLabel = FindNode("WeaponsLabel") as Label;
            propulsionLabel = FindNode("PropulsionLabel") as Label;
            constructionLabel = FindNode("ConstructionLabel") as Label;
            electronicsLabel = FindNode("ElectronicsLabel") as Label;
            biotechnologyLabel = FindNode("BiotechnologyLabel") as Label;

            energyReqLabel = FindNode("EnergyReqLabel") as Label;
            weaponsReqLabel = FindNode("WeaponsReqLabel") as Label;
            propulsionReqLabel = FindNode("PropulsionReqLabel") as Label;
            constructionReqLabel = FindNode("ConstructionReqLabel") as Label;
            electronicsReqLabel = FindNode("ElectronicsReqLabel") as Label;
            biotechnologyReqLabel = FindNode("BiotechnologyReqLabel") as Label;

            unavailableLabel = FindNode("UnavailableLabel") as Label;
            labelTraitRequirement = FindNode("LabelTraitRequirement") as Label;

            // these containers hold information about our tech
            statsContainer = FindNode("StatsContainer") as Container;
            descriptionContainer = FindNode("DescriptionContainer") as Container;

            // engines have a graph
            engineGraph = FindNode("EngineGraph") as EngineGraph;

            // defenses have a graph
            defenseGraph = FindNode("DefenseGraph") as DefenseGraph;

            iconTextureRect.Connect("gui_input", this, nameof(OnIconGUIInput));

        }

        void OnIconGUIInput(InputEvent @event)
        {
            if (Tech != null && Tech is TechHull hull && @event.IsActionPressed("hullcomponent_alternate_select"))
            {
                GetTree().SetInputAsHandled();

                HullSummaryPopup.Instance.Hull = hull;
                HullSummaryPopup.Instance.ShipDesign = null;
                HullSummaryPopup.ShowAtMouse();
            }
            else if (@event.IsActionReleased("hullcomponent_alternate_select"))
            {
                HullSummaryPopup.Instance.Hide();
            }

        }

        /// <summary>
        /// Change the active tech
        /// </summary>
        void UpdateControls()
        {
            if (Tech != null && descriptionContainer != null)
            {
                descriptionContainer.Visible = true;
                engineGraph.Visible = false;
                defenseGraph.Visible = false;
                nameLabel.Text = Tech.Name;
                costGrid.Cost = Tech.Cost;
                if (Tech is TechHull)
                {
                    iconTextureRect.Texture = TextureLoader.Instance.FindTexture(Tech, 0);
                }
                else
                {
                    iconTextureRect.Texture = TextureLoader.Instance.FindTexture(Tech);
                }

                UpdateRequirements(Tech);

                // clear out any stats in the grid
                foreach (Node child in statsContainer.GetChildren())
                {
                    child.QueueFree();
                }

                // clear out any descriptions
                foreach (Node child in descriptionContainer.GetChildren())
                {
                    child.QueueFree();
                }

                if (Tech is TechHull hull)
                {
                    massLabel.Visible = massAmountLabel.Visible = false;
                    if (hull.FuelCapacity > 0)
                    {
                        AddStatsLabel("Fuel Capacity", $"{hull.FuelCapacity}mg");
                    }
                    if (hull.CargoCapacity > 0)
                    {
                        AddStatsLabel("Cargo Capacity", $"{hull.CargoCapacity}kT");
                    }
                    AddStatsLabel("Armor Strength", hull.Armor.ToString());
                    AddStatsLabel("Initiative", hull.Initiative.ToString());

                }
                else if (Tech is TechPlanetaryScanner planetaryScanner)
                {
                    massLabel.Visible = massAmountLabel.Visible = false;
                    if (planetaryScanner.ScanRange > 0)
                    {
                        AddDescription($"Enemy fleets not orbiting a planet can be detected up to {planetaryScanner.ScanRange} light years away.");
                    }

                    if (planetaryScanner.ScanRangePen > 0)
                    {
                        AddDescription($"This scanner can determine a planet's basic stats from a distance up to {planetaryScanner.ScanRangePen} light years. The scanner will also spot enemy fleets attempting to hide behind planets within range.");
                    }

                }
                else if (Tech is TechTerraform terraform)
                {
                    massLabel.Visible = massAmountLabel.Visible = false;
                    if (terraform.HabType == TerraformHabType.All)
                    {
                        AddDescription($"Allows you to modify any of a planet's three environment variables up to {terraform.Ability}% from its original value");
                    }
                    else
                    {
                        AddDescription($"Allows you to modify a planet's {terraform.HabType} by up to {terraform.Ability}% from its original value");
                    }

                }
                else if (Tech is TechEngine engine)
                {
                    descriptionContainer.Visible = false;
                    engineGraph.Visible = true;
                    engineGraph.Engine = engine;
                    engineGraph.UpdateControls();
                }
                else if (Tech is TechDefense defense)
                {
                    descriptionContainer.Visible = false;
                    defenseGraph.Visible = true;
                    defenseGraph.Defense = defense;
                    defenseGraph.UpdateControls();
                }
                else if (Tech is TechHullComponent hullComponent)
                {
                    massLabel.Visible = massAmountLabel.Visible = true;
                    massAmountLabel.Text = $"{hullComponent.Mass}kT";

                    if (hullComponent.Category == TechCategory.MineLayer)
                    {
                        var mineFieldStats = GameInfo.Rules.MineFieldStatsByType[hullComponent.MineFieldType];
                        AddStatsLabel("Mines laid per year", $"{hullComponent.MineLayingRate}");
                        AddStatsLabel("Maximum safe speed", $"{mineFieldStats.MaxSpeed}");
                        AddStatsLabel("Chance/l.y. of a hit", $"{mineFieldStats.ChanceOfHit * 100}%");
                        AddStatsLabel("Dmg done to each ship", $"{mineFieldStats.DamagePerEngine} ({mineFieldStats.DamagePerEngineRS}) / engine");
                        AddStatsLabel("Min damage done to fleet", $"{mineFieldStats.MinDamagePerFleet} ({mineFieldStats.MinDamagePerFleetRS})");
                        AddDescription("Numbers in parenthesis are for fleets containing a ship with ram scoop engines. Note that the chance of hitting a mine goes up the % listed for EACH warp you exceed the safe speed.");
                    }

                    if (hullComponent.Category == TechCategory.Shield && hullComponent.Armor > 0)
                    {
                        // if this is a shield with armor, it sounds cooler to make the armor a description
                        AddDescription($"This shield also contains an armor component which will absorb {hullComponent.Armor} damage points.");
                    }
                    else if (hullComponent.Armor > 0)
                    {
                        AddStatsLabel("Armor Strength", hullComponent.Armor.ToString());
                    }

                    if (hullComponent.Category == TechCategory.Armor && hullComponent.Shield > 0)
                    {
                        // if this is an armor with a shield, it sounds cooler to make the shield a description
                        AddDescription($"This armor also acts as part shield which will absorb {hullComponent.Shield} damage points.");
                    }
                    else if (hullComponent.Shield > 0)
                    {
                        AddStatsLabel("Shield Strength", hullComponent.Shield.ToString());
                    }

                    if (hullComponent.Power > 0)
                    {
                        AddStatsLabel("Power", hullComponent.Power.ToString());
                    }
                    if (hullComponent.Range > 0 || hullComponent.Category == TechCategory.BeamWeapon)
                    {
                        AddStatsLabel("Range", hullComponent.Range.ToString());
                    }
                    if (hullComponent.Initiative > 0)
                    {
                        AddStatsLabel("Initiative", hullComponent.Initiative.ToString());
                    }
                    if (hullComponent.HitsAllTargets)
                    {
                        AddDescription($"This weapon hits all targets in range each time it is fired.");
                    }
                    if (hullComponent.Gattling)
                    {
                        AddDescription($"This weapon also makes an excellent mine sweeper, capable of sweeping {hullComponent.Power * Math.Pow(hullComponent.Range, 4)} mines per year.");
                    }
                    if (hullComponent.DamageShieldsOnly)
                    {
                        AddDescription($"This weapon will only damage shields, it has no effect on armor.");
                    }

                    if (hullComponent.KillRate > 0 && !hullComponent.OrbitalConstructionModule)
                    {
                        // we have special text for orbital construction modules.
                        AddDescription($"This bomb will kill approimately {hullComponent.KillRate}% of a planet's populatation each year.");
                        if (hullComponent.MinKillRate > 0)
                        {
                            AddDescription($"If a planet has no defenses, this bomb is guaranteed to kill at least {hullComponent.MinKillRate} colonists.");
                        }
                        if (hullComponent.StructureDestroyRate == 0)
                        {
                            AddDescription("This bomb will not damage a planet's mines or factories.");
                        }
                    }

                    if (hullComponent.MiningRate > 0)
                    {
                        AddDescription($"This module contains robots capable of mining up to {hullComponent.MiningRate}kT of each mineral (depending on concentration) from an uninhabited planet the ship is orbiting. The fleet must have orders set to 'Remote Mining'.");
                    }

                    if (hullComponent.TerraformRate > 0)
                    {
                        AddDescription($"This modified mining robot terraforms inhabited planets by {hullComponent.TerraformRate} per year. It has a positive effect on friendly planets, a negative effect on neutral and enemy planets.");

                        if (hullComponent.CloakUnits > 0)
                        {
                            AddDescription($"It also provides {hullComponent.CloakUnits}% cloaking.");
                        }
                    }

                    if (hullComponent.StructureDestroyRate > 0)
                    {
                        AddDescription($"This bomb will destroy approximately {hullComponent.StructureDestroyRate} of a planet's mines, factories, and/or defenses each year.");
                    }

                    if (hullComponent.UnterraformRate > 0)
                    {
                        AddDescription($"This bomb does not kill colonists or destroy installations. This bomb 'unterraforms' planets toward their original state up to {hullComponent.TerraformRate}% per variable per bombing run. Planetary defenses have no effect on this bomb.");
                    }

                    if (hullComponent.CloakUnits > 0 && hullComponent.TerraformRate == 0)
                    {
                        if (hullComponent.CloakUnarmedOnly)
                        {
                            AddDescription($"Cloaks unarmed hulls, reducing the range at which scanners detect it by up to {CloakUtils.GetCloakPercentForCloakUnits(hullComponent.CloakUnits)}%.");
                        }
                        else
                        {
                            AddDescription($"Cloaks any ship, reducing the range at which scanners detect it by up to {CloakUtils.GetCloakPercentForCloakUnits(hullComponent.CloakUnits)}%.");
                        }
                    }

                    if (hullComponent.FuelBonus > 0)
                    {
                        AddDescription($"This part acts as a {hullComponent.FuelBonus}mg fuel tank.");
                    }

                    if (hullComponent.FuelRegenerationRate > 0)
                    {
                        AddDescription($"This part generates {hullComponent.FuelRegenerationRate}mg of fuel every year.");
                    }

                    if (hullComponent.ColonizationModule)
                    {
                        AddDescription("This pod allows a ship to colonize a planet and will dismantle the ship upon arrival and convert it into supplies for the colonists.");
                    }

                    if (hullComponent.OrbitalConstructionModule)
                    {
                        AddDescription("This module contains an empty orbital hull which can be deployed in orbit of an uninhabited planet.");
                        if (hullComponent.MinKillRate > 0)
                        {
                            AddDescription($"This pod also contains viral weapons capable of killing {hullComponent.MinKillRate} enemy colonists per attack.");
                        }
                    }

                    if (hullComponent.CargoBonus > 0)
                    {
                        AddDescription($"This pod increases the cargo capacity of the ship by {hullComponent.CargoBonus}kT");
                    }

                    if (hullComponent.MovementBonus > 0)
                    {
                        AddDescription($"Increases speed in battle by {hullComponent.MovementBonus} square of movement.");
                    }

                    if (hullComponent.BeamDefense > 0)
                    {
                        AddDescription($"The deflector decreases damage done by beam weapons to this ship by up to {hullComponent.BeamDefense}%");
                    }

                    if (hullComponent.TorpedoBonus > 0 || hullComponent.InitiativeBonus > 0)
                    {
                        if (hullComponent.TorpedoBonus > 0 && hullComponent.InitiativeBonus > 0)
                        {
                            AddDescription($"This module increases the accuracy of your torpedos by {hullComponent.TorpedoBonus}% and increases your initiative by {hullComponent.InitiativeBonus}. If an enemy ship has jammers the computer acts to offset their effects.");
                        }
                        else if (hullComponent.InitiativeBonus > 0)
                        {
                            AddDescription($"This module increases your initiative by {hullComponent.InitiativeBonus}.");
                        }
                        else if (hullComponent.TorpedoBonus > 0)
                        {
                            AddDescription($"This module increases the accuracy of your torpedos by {hullComponent.TorpedoBonus}%. If an enemy ship has jammers the computer acts to offset their effects.");
                        }
                    }

                    if (hullComponent.TorpedoJamming > 0)
                    {
                        AddDescription($"Has a {hullComponent.TorpedoJamming * 100}% chance of deflecting incoming torpedos. Deflected torpedoes will still reduce shields (in any by 1/8 the damage value).");
                    }

                    if (hullComponent.BeamBonus > 0)
                    {
                        AddDescription($"Increases the damage done by all beam weapons on their ship by {hullComponent.BeamBonus * 100}%.");
                    }

                    if (hullComponent.ReduceMovement > 0)
                    {
                        AddDescription($"Slows all ships in combat by {hullComponent.ReduceMovement} square of movement.");
                    }

                    if (hullComponent.ReduceCloaking)
                    {
                        AddDescription($"Reduces the effectiveness of other players cloaks by {GameInfo.Rules.TachyonCloakReduction}%.");
                    }

                    if (hullComponent.SafeRange > 0)
                    {
                        AddDescription($"Allows fleets without cargo to jump to any other planet with a Stargate in a single year.");
                        AddStatsLabel("Safe hull mass", hullComponent.SafeHullMass == TechHullComponent.InfinteGate ? "Unlimited" : $"{hullComponent.SafeHullMass}kT");
                        AddStatsLabel("Safe range", hullComponent.SafeRange == TechHullComponent.InfinteGate ? "Unlimited" : $"{hullComponent.SafeRange} light years");

                        if (hullComponent.MaxHullMass != TechHullComponent.InfinteGate && hullComponent.MaxRange != TechHullComponent.InfinteGate)
                        {
                            AddWarning($"Warning: Ships up to {hullComponent.MaxHullMass}kT might be successfully gated up to {hullComponent.MaxRange} l.y. but exceeding the stated limits will cause damage to the fleet.");
                        }
                        else if (hullComponent.MaxHullMass != TechHullComponent.InfinteGate)
                        {
                            AddWarning($"Warning: Ships up to {hullComponent.MaxHullMass}kT can be successfully gated up but exceeding the stated limits will cause damage to the fleet.");
                        }
                        else if (hullComponent.MaxRange != TechHullComponent.InfinteGate)
                        {
                            AddWarning($"Warning: Ships can be successfully gated up to {hullComponent.MaxRange} l.y. but exceeding the stated limits will cause damage to the fleet.");
                        }
                    }

                    if (hullComponent.PacketSpeed > 0)
                    {
                        AddStatsLabel("Warp", $"{hullComponent.PacketSpeed}");
                        AddDescription("Allows planets to fling mineral packets at other planets.");
                        AddWarning("Warning: The receiving planet must have a mass driver at least as capable or it will take damage.");
                    }

                    if (hullComponent.ScanRange > 0)
                    {
                        if (hullComponent.ScanRange > 0)
                        {
                            if (hullComponent.ScanRange == TechHullComponent.ScanWithZeroRange)
                            {
                                // special case for bat scanner
                                AddDescription($"Enemy fleets cannot be detected boy this scanner unless they are at the same location as the scanner.");
                            }
                            else
                            {
                                AddDescription($"Enemy fleets not orbiting a planet can be detected up to {hullComponent.ScanRange} light years away.");
                            }

                            if (hullComponent.ScanRangePen == 0)
                            {
                                // we have no pen scan, but we are a normal scanner, we can still scan planets we orbit
                                AddDescription($"This scanner is capable of determining a planet's environment and composition while in orbit of the planet.");
                            }
                        }

                        if (hullComponent.ScanRangePen > 0)
                        {
                            AddDescription($"This scanner can determine a planet's basic stats from a distance up to {hullComponent.ScanRangePen} light years. The scanner will also spot enemy fleets attempting to hide behind planets within range.");
                        }

                        if (hullComponent.StealCargo)
                        {
                            AddDescription($"This scanner is capable of penetrating the defenses of enemy fleets and planets allowing you to steal their cargo.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add a stats label, like "Power: 10" for this tech
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void AddStatsLabel(string name, string value)
        {
            statsContainer.AddChild(new Label()
            {
                Text = name + ":",
                SizeFlagsHorizontal = (int)Godot.Control.SizeFlags.ExpandFill,
                Align = Label.AlignEnum.Right,
            });
            statsContainer.AddChild(new Label()
            {
                Text = value,
                SizeFlagsHorizontal = (int)Godot.Control.SizeFlags.ExpandFill
            });
        }

        /// <summary>
        /// Add a descriptive label for this tech
        /// </summary>
        /// <param name="description"></param>
        void AddDescription(string description)
        {
            descriptionContainer.AddChild(new Label()
            {
                Text = description,
                Autowrap = true
            });
        }

        /// <summary>
        /// Add a descriptive label for this tech
        /// </summary>
        /// <param name="description"></param>
        void AddWarning(string description)
        {
            descriptionContainer.AddChild(new Label()
            {
                Text = description,
                Autowrap = true,
                Modulate = Colors.Red
            });
        }

        /// <summary>
        /// Update all the tech requirements fields
        /// </summary>
        /// <param name="tech"></param>
        void UpdateRequirements(Tech tech)
        {
            // if this tech is never learnable by us, make it invisible
            unavailableLabel.Visible = !playerTechService.CanLearnTech(Me, tech);

            var reqs = tech.Requirements;
            // We only show things > 0
            energyLabel.Visible = energyReqLabel.Visible = reqs.Energy > 0;
            weaponsLabel.Visible = weaponsReqLabel.Visible = reqs.Weapons > 0;
            propulsionLabel.Visible = propulsionReqLabel.Visible = reqs.Propulsion > 0;
            constructionLabel.Visible = constructionReqLabel.Visible = reqs.Construction > 0;
            electronicsLabel.Visible = electronicsReqLabel.Visible = reqs.Electronics > 0;
            biotechnologyLabel.Visible = biotechnologyReqLabel.Visible = reqs.Biotechnology > 0;

            // show these if we have no level requirements
            noneLabel.Visible = spacerLabel.Visible =
                !energyLabel.Visible &&
                !weaponsLabel.Visible &&
                !propulsionLabel.Visible &&
                !constructionLabel.Visible &&
                !electronicsLabel.Visible &&
                !biotechnologyLabel.Visible;

            energyReqLabel.Text = reqs.Energy.ToString();
            weaponsReqLabel.Text = reqs.Weapons.ToString();
            propulsionReqLabel.Text = reqs.Propulsion.ToString();
            constructionReqLabel.Text = reqs.Construction.ToString();
            electronicsReqLabel.Text = reqs.Electronics.ToString();
            biotechnologyReqLabel.Text = reqs.Biotechnology.ToString();

            // TODO: compute research cost for this tech
            researchCostLabel.Visible = false;

            labelTraitRequirement.Text = "";
            labelTraitRequirement.Modulate = Colors.White;
            if (reqs.PRTRequired != PRT.None)
            {
                labelTraitRequirement.Text += $"This part requires the Primary Racial trait '{EnumUtils.GetLabelForPRT(reqs.PRTRequired)}'  ";
                if (Me.Race.PRT != reqs.PRTRequired)
                {
                    labelTraitRequirement.Modulate = Colors.Red;
                }
            }
            if (reqs.PRTDenied != PRT.None)
            {
                labelTraitRequirement.Text += $"This part will not be available if the Primary Racial trait '{EnumUtils.GetLabelForPRT(reqs.PRTDenied)}'";
                if (Me.Race.PRT == reqs.PRTDenied)
                {
                    labelTraitRequirement.Modulate = Colors.Red;
                }
            }

            foreach (LRT lrt in reqs.LRTsRequired)
            {
                labelTraitRequirement.Text += $"This part requires the Lesser Racial trait '{EnumUtils.GetLabelForLRT(lrt)}'";

                if (!Me.Race.HasLRT(lrt))
                {
                    labelTraitRequirement.Modulate = Colors.Red;
                }
            }

            foreach (LRT lrt in reqs.LRTsDenied)
            {
                labelTraitRequirement.Text += $"This part will be unavailable if you have the Lesser Racial trait '{EnumUtils.GetLabelForLRT(lrt)}'";

                if (Me.Race.HasLRT(lrt))
                {
                    labelTraitRequirement.Modulate = Colors.Red;
                }
            }

        }

    }
}