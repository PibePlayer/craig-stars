using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public abstract class GameViewDialog : WindowDialog
    {
        protected Player Me { get => PlayersManager.Me; }
        protected PublicGameInfo GameInfo { get => PlayersManager.GameInfo; }
        protected Button okButton;
        Action onOk;

        public override void _Ready()
        {
            base._Ready();

            okButton = GetNode("MarginContainer/VBoxContainer/HBoxContainerButtons/HBoxContainerOKButton/OKButton") as Button;
            // TODO: once we migrate dialogs, remove this
            okButton ??= FindNode("OKButton") as Button;
            okButton?.Connect("pressed", this, nameof(OnOk));

            Connect("visibility_changed", this, nameof(OnVisibilityChanged));

            // we are debugging, so show the dialog
            if (GetParent() == GetTree().Root)
            {
                PlayersManager.Me = new Player();
                PlayersManager.GameInfo = new PublicGameInfo();

                CallDeferred(nameof(ShowOnStart));
            }
        }

        void ShowOnStart()
        {
            base.PopupCentered();
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

        public virtual void PopupCentered(Action onOk = null)
        {
            this.onOk = onOk;
            base.PopupCentered();
        }

        /// <summary>
        /// Just hide the dialog on ok
        /// </summary>
        protected virtual void OnOk()
        {
            Hide();
            onOk?.Invoke();
        }

        void DecrementDialogRefCount()
        {
            DialogManager.DialogRefCount--;
        }
    }
}