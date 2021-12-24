using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class TechBrowserDialog : GameViewDialog
    {
        Tech SelectedTech;
        TechTree techTree;
        TechSummary techSummary;

        public override void _Ready()
        {
            base._Ready();
            techTree = GetNode<TechTree>("MarginContainer/VBoxContainer/ContentContainer/HBoxContainer/TechTree");
            techSummary = GetNode<TechSummary>("MarginContainer/VBoxContainer/ContentContainer/HBoxContainer/TechSummary");

            techTree.TechSelectedEvent += OnTechSelected;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                techTree.TechSelectedEvent -= OnTechSelected;
            }
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();
            if (IsVisibleInTree())
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