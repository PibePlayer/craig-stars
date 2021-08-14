using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars.Client
{
    /// <summary>
    /// Abstract Base class for dialog components. Handles dialog refcounts and automatic hiding
    /// when the Ok button is pressed.
    /// </summary>
    public abstract class CSDialog : GameViewDialog
    {
        Button okButton;

        public override void _Ready()
        {
            base._Ready();
            okButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainerButtons/OKButton");

            okButton.Connect("pressed", this, nameof(OnOk));
        }

        /// <summary>
        /// Just hide the dialog on ok
        /// </summary>
        protected virtual void OnOk()
        {
            Hide();
        }

    }
}