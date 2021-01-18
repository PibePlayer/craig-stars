using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;

namespace CraigStars
{
    public class TechBrowserDialog : WindowDialog
    {
        Tree techTree;
        TreeItem root;

        Label nameLabel;
        TextureRect iconTextureRect;
        CostGrid costGrid;

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

        List<Tech> techs = new List<Tech>();

        Player me;

        public override void _Ready()
        {
            techTree = FindNode("TechTree") as Tree;
            costGrid = FindNode("CostGrid") as CostGrid;
            nameLabel = FindNode("NameLabel") as Label;
            iconTextureRect = FindNode("IconTextureRect") as TextureRect;

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

            // this is a
            statsContainer = FindNode("StatsContainer") as Container;
            descriptionContainer = FindNode("DescriptionContainer") as Container;

            me = PlayersManager.Instance.Me;

            root = techTree.CreateItem();

            techTree.Connect("item_selected", this, nameof(OnTechSelected));
            AddTechsToTree();

        }

        void AddTechsToTree()
        {
            techs = new List<Tech>(TechStore.Instance.Techs);
            techs.Sort((t1, t2) => t1.Ranking.CompareTo(t2.Ranking));
            Dictionary<TechCategory, TreeItem> categoryItemByCategory = new Dictionary<TechCategory, TreeItem>();

            // get a list of categories, sorted.
            var categories = techs.Select(tech => tech.Category).Distinct().ToList();
            categories.Sort();
            foreach (var category in categories)
            {
                var categoryTreeItem = techTree.CreateItem(root);
                categoryTreeItem.SetText(0, category.ToString());
                categoryItemByCategory[category] = categoryTreeItem;
            }

            techs.Each((tech, index) =>
            {
                var categoryRoot = categoryItemByCategory[tech.Category];
                var item = techTree.CreateItem(categoryRoot);
                item.SetMetadata(0, index);
                item.SetText(0, tech.Name);
                item.SetIcon(0, TextureLoader.Instance.FindTexture(tech));
            });

            // select the first item in the first category
            var firstCategoryTreeItem = categoryItemByCategory[categories[0]] as TreeItem;
            firstCategoryTreeItem.GetChildren()?.Select(0);

        }

        /// <summary>
        /// Just hide the dialog on ok
        /// </summary>
        void OnOk()
        {
            Hide();
        }

        /// <summary>
        /// Change the active tech
        /// </summary>
        void OnTechSelected()
        {
            var selected = techTree.GetSelected();
            if (selected.GetMetadata(0) is int index)
            {
                var tech = techs[index];
                nameLabel.Text = tech.Name;
                costGrid.Cost = tech.Cost;
                iconTextureRect.Texture = TextureLoader.Instance.FindTexture(tech);

                UpdateRequirements(tech);

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

                if (tech is TechHull hull)
                {
                    massAmountLabel.Visible = false;
                }
                else if (tech is TechHullComponent hullComponent)
                {
                    massAmountLabel.Visible = true;
                    massAmountLabel.Text = $"{hullComponent.Mass}kT";

                    if (hullComponent.Armor > 0)
                    {
                        AddStatsLabel("Armor Strength", hullComponent.Armor.ToString());
                    }
                    if (hullComponent.Power > 0)
                    {
                        AddStatsLabel("Power", hullComponent.Power.ToString());
                    }
                    if (hullComponent.Range > 0)
                    {
                        AddStatsLabel("Range", hullComponent.Range.ToString());
                    }
                    if (hullComponent.Initiative > 0)
                    {
                        AddStatsLabel("Initiative", hullComponent.Initiative.ToString());
                    }

                    if (hullComponent.Cloak > 0)
                    {
                        if (hullComponent.CloakUnarmedOnly)
                        {
                            AddDescription($"Cloaks unarmed hulls, reducing the range at which scanners detect it by up to {hullComponent.Cloak}%.");
                        }
                        else
                        {
                            AddDescription($"Cloaks any ship, reducing the range at which scanners detect it by up to {hullComponent.Cloak}%.");
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
                Text = name,
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
            unavailableLabel.Visible = !me.CanLearnTech(tech);

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
                labelTraitRequirement.Text += $"This part requires the Primary Racial trait '{GetPRTFullName(reqs.PRTRequired)}'  ";
                if (me.Race.PRT != reqs.PRTRequired)
                {
                    labelTraitRequirement.Modulate = Colors.Red;
                }
            }
            if (reqs.PRTDenied != PRT.None)
            {
                labelTraitRequirement.Text += $"This part will not be available if the Primary Racial trait '{GetPRTFullName(reqs.PRTRequired)}'";
                if (me.Race.PRT == reqs.PRTDenied)
                {
                    labelTraitRequirement.Modulate = Colors.Red;
                }
            }

            foreach (LRT lrt in reqs.LRTsRequired)
            {
                labelTraitRequirement.Text += $"This part requires the Lesser Racial trait '{GetLRTFullName(lrt)}'";

                if (!me.Race.HasLRT(lrt))
                {
                    labelTraitRequirement.Modulate = Colors.Red;
                }
            }

            foreach (LRT lrt in reqs.LRTsDenied)
            {
                labelTraitRequirement.Text += $"This part will be unavailable if you have the Lesser Racial trait '{GetLRTFullName(lrt)}'";

                if (me.Race.HasLRT(lrt))
                {
                    labelTraitRequirement.Modulate = Colors.Red;
                }
            }

        }

        string GetPRTFullName(PRT prt)
        {
            switch (prt)
            {
                case PRT.HE:
                    return "Hyper Expansion";
                case PRT.SS:
                    return "Super Stealth";
                case PRT.WM:
                    return "Warmonger";
                case PRT.CA:
                    return "Claim Adjuster";
                case PRT.IS:
                    return "Inner Strength";
                case PRT.SD:
                    return "Space Demolition";
                case PRT.PP:
                    return "Packet Physics";
                case PRT.IT:
                    return "Interstellar Traveler";
                case PRT.AR:
                    return "Alternate Reality";
                case PRT.JoaT:
                    return "Jack of All Trades";
                default:
                    return prt.ToString();
            }
        }

        string GetLRTFullName(LRT lrt)
        {
            switch (lrt)
            {
                case LRT.IFE:
                    return "Improved Fuel Efficiency";
                case LRT.TT:
                    return "Total Terraforming";
                case LRT.ARM:
                    return "Advanced Remote Mining";
                case LRT.ISB:
                    return "Improved Starbases";
                case LRT.GR:
                    return "Generalized Research";
                case LRT.UR:
                    return "Ultimate Recycling";
                case LRT.NRSE:
                    return "No Ramscoop Engines";
                case LRT.OBRM:
                    return "Only Basic Remote Mining";
                case LRT.NAS:
                    return "No Advanced Scanners";
                case LRT.LSP:
                    return "Low Starting Population";
                case LRT.BET:
                    return "Bleeding Edge Technology";
                case LRT.RS:
                    return "Regenerating Shields";
                case LRT.MA:
                    return "Mineral Alchemy";
                case LRT.CE:
                    return "Cheap Engines";
                default:
                    return lrt.ToString();
            }
        }
    }
}