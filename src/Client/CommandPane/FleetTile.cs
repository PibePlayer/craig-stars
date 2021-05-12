using Godot;
using System;
using CraigStars.Singletons;
using CraigStars;
using System.Collections.Generic;

namespace CraigStars.Client
{
    public class FleetTile : AbstractCommandedFleetControl, ITileContent
    {

        public event UpdateTitleAction UpdateTitleEvent;
        public event UpdateVisibilityAction UpdateVisibilityEvent;

        public override void _Ready()
        {
            base._Ready();
        }

        protected void UpdateTitle(string title)
        {
            UpdateTitleEvent?.Invoke(title);
        }

        /// <summary>
        /// Called when a new active fleet has been selected
        /// Note, this will be called when setting the CommandedFleet to null
        /// </summary>
        protected override void OnNewCommandedFleet() { }

        protected override void UpdateControls()
        {
            Visible = CommandedFleet != null;
            UpdateVisibilityEvent?.Invoke(Visible);
        }

    }
}