using CraigStars.Singletons;
using Godot;
using System;
using System.Linq;

namespace CraigStars
{
    public class ReportsDialog : GameViewDialog
    {
        Button okButton;

        public override void _Ready()
        {
            base._Ready();
            okButton = FindNode("OKButton") as Button;
            okButton.Connect("pressed", this, nameof(OnOK));
        }

        void OnOK() => Hide();
    }
}