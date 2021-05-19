using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;

namespace CraigStars
{
    public class TechSummary : Control
    {
        protected Player Me { get => PlayersManager.Me; }

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

        public override void _Ready()
        {
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

            iconTextureRect.Connect("gui_input", this, nameof(OnIconGUIInput));

        }

        void OnIconGUIInput(InputEvent @event)
        {
            if (Tech != null && Tech is TechHull hull && @event.IsActionPressed("viewport_select"))
            {
                GetTree().SetInputAsHandled();

                HullSummaryPopup.Instance.Hull = hull;
                HullSummaryPopup.Instance.PopupCentered();
            }
        }

        /// <summary>
        /// Change the active tech
        /// </summary>
        void UpdateControls()
        {
            if (Tech != null)
            {
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
                    AddStatsLabel("Fuel Capacity", $"{hull.FuelCapacity}mg");
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
                else if (Tech is TechHullComponent hullComponent)
                {
                    massLabel.Visible = massAmountLabel.Visible = true;
                    massAmountLabel.Text = $"{hullComponent.Mass}kT";

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

                    if (hullComponent.StructureDestroyRate > 0)
                    {
                        AddDescription($"This bomb will destroy approximately {hullComponent.StructureDestroyRate} of a planet's mines, factories, and/or defenses each year.");
                    }

                    if (hullComponent.TerraformRate > 0)
                    {
                        AddDescription($"This bomb does not kill colonists or destroy installations. This bomb 'unterraforms' planets toward their original state up to {hullComponent.TerraformRate}% per variable per bombing run. Planetary defenses have no effect on this bomb.");
                    }

                    if (hullComponent.CloakUnits > 0)
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
        /// Update all the tech requirements fields
        /// </summary>
        /// <param name="tech"></param>
        void UpdateRequirements(Tech tech)
        {
            // if this tech is never learnable by us, make it invisible
            unavailableLabel.Visible = !Me.CanLearnTech(tech);

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