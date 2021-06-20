using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// Abstract Base class for dialog components. Handles dialog refcounts and automatic hiding
    /// when the Ok button is pressed.
    /// </summary>
    public abstract class CSDialog : WindowDialog
    {
        protected Player Me { get => PlayersManager.Me; }
        Button okButton;

        public override void _Ready()
        {
            base._Ready();
            okButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainerButtons/OKButton");

            okButton.Connect("pressed", this, nameof(OnOk));
            Connect("visibility_changed", this, nameof(OnVisibilityChanged));
        }

        /// <summary>
        /// Just hide the dialog on ok
        /// </summary>
        protected virtual void OnOk()
        {
            Hide();
        }

        /// <summary>
        /// When the dialog becomes visible, update the controls for this player
        /// </summary>
        protected virtual void OnVisibilityChanged()
        {
            if (IsVisibleInTree())
            {
                DialogManager.DialogRefCount++;
            }
            else
            {
                CallDeferred(nameof(DecrementDialogRefCount));
            }
        }

        void DecrementDialogRefCount()
        {
            DialogManager.DialogRefCount--;
        }

    }
}