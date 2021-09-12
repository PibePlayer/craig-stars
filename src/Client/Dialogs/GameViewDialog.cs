using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars.Client
{
    public abstract class GameViewDialog : WindowDialog
    {
        protected Player Me { get => PlayersManager.Me; }
        protected PublicGameInfo GameInfo { get => PlayersManager.GameInfo; }
        protected Button okButton;

        public override void _Ready()
        {
            base._Ready();

            okButton = GetNode("MarginContainer/VBoxContainer/HBoxContainerButtons/HBoxContainerOKButton/OKButton") as Button;
            // TODO: once we migrate dialogs, remove this
            okButton ??= FindNode("OKButton") as Button;
            okButton?.Connect("pressed", this, nameof(OnOk));

            Connect("visibility_changed", this, nameof(OnVisibilityChanged));
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

        /// <summary>
        /// Just hide the dialog on ok
        /// </summary>
        protected virtual void OnOk()
        {
            Hide();
        }

        void DecrementDialogRefCount()
        {
            DialogManager.DialogRefCount--;
        }


    }
}