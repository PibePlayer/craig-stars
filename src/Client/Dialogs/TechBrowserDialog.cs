using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;

namespace CraigStars
{
    public class TechBrowserDialog : GameViewDialog
    {
        Tech SelectedTech;
        TechTree techTree;
        TechSummary techSummary;

        public override void _Ready()
        {
            base._Ready();
            techTree = GetNode<TechTree>("MarginContainer/VBoxContainer/HBoxContainerContent/TechTree");
            techSummary = GetNode<TechSummary>("MarginContainer/VBoxContainer/HBoxContainerContent/TechSummary");

            techTree.TechSelectedEvent += OnTechSelected;
        }

        public override void _ExitTree()
        {
            techTree.TechSelectedEvent -= OnTechSelected;
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();
            if (Visible)
            {
                techTree.FocusSearch();
            }
        }

        /// <summary>
        /// Change the active tech
        /// </summary>
        void OnTechSelected(Tech tech)
        {
            techSummary.Tech = tech;
        }
    }
}