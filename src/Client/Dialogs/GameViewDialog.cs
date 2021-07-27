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
        protected PublicGameInfo GameInfo { get; set; }

        public override void _Ready()
        {
            base._Ready();
            Connect("visibility_changed", this, nameof(OnVisibilityChanged));

            EventManager.GameViewResetEvent += OnGameViewReset;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            EventManager.GameViewResetEvent -= OnGameViewReset;
        }

        private void OnGameViewReset(PublicGameInfo gameInfo)
        {
            GameInfo = gameInfo;
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