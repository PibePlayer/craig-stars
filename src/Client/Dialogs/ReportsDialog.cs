using CraigStars.Singletons;
using Godot;
using System;
using System.Linq;

namespace CraigStars
{
    public class ReportsDialog : WindowDialog
    {
        /// <summary>
        /// Show the admin view, which shows all planets and fleets from the game
        /// </summary>
        /// <value></value>
        public bool AdminView { get; set; }

        public override void _Ready()
        {

        }

        void ShowMe()
        {
            PopupCentered();
        }

    }
}