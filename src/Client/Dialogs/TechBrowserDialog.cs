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
        Button okButton;

        public override void _Ready()
        {
            base._Ready();
            okButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainerButtons/OKButton");
            techTree = GetNode<TechTree>("MarginContainer/VBoxContainer/HBoxContainerContent/TechTree");
            techSummary = GetNode<TechSummary>("MarginContainer/VBoxContainer/HBoxContainerContent/TechSummary");

            okButton.Connect("pressed", this, nameof(OnOk));

            techTree.TechSelectedEvent += OnTechSelected;

            // PlayersManager.Instance.SetupPlayers();
            // Show();
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
        /// Just hide the dialog on ok
        /// </summary>
        void OnOk()
        {
            Hide();
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