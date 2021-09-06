using CraigStarsTable;
using Godot;
using System;

namespace CraigStars.Client
{
    /// <summary>
    /// Custom button type to allow for an event based button
    /// </summary>
    public class CSButton : Button
    {
        Action<CSButton> onPressed;

        public override void _Ready()
        {
            base._Ready();
            Connect("pressed", this, nameof(OnPressedCallback));
        }

        void OnPressedCallback()
        {
            onPressed?.Invoke(this);
        }

        /// <summary>
        /// Register a callback with the "pressed" signal for this button
        /// </summary>
        /// <param name="onPressed">The action to callback</param>
        public void OnPressed(Action<CSButton> onPressed)
        {
            this.onPressed = onPressed;
        }

    }
}