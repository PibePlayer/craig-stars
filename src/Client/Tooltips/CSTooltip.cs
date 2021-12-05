using System;
using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client
{
    public abstract class CSTooltip : PopupPanel
    {
        protected Player Me { get => PlayersManager.Me; }
        public Planet Planet { get; set; }

        protected abstract void UpdateControls();

        public void ShowAtMouse(Planet planet)
        {
            Planet = planet;
            UpdateControls();

            var mousePos = GetGlobalMousePosition();
            var yPos = mousePos.y - RectSize.y;
            RectPosition = new Vector2(mousePos.x, Mathf.Clamp(yPos, 0, GetViewportRect().Size.y - RectSize.y));

            Show();
        }

    }
}