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

        public override void _Ready()
        {
            base._Ready();
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

        void DecrementDialogRefCount()
        {
            DialogManager.DialogRefCount--;
        }


    }
}