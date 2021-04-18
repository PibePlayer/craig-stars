using Godot;
using System;

namespace CraigStars
{

    public class SalvageSprite : MapObjectSprite
    {
        [Export]
        public GUIColors GUIColors { get; set; } = new GUIColors();

        /// <summary>
        /// Convenience method so the code looks like Fleet.Something instead of MapObject.Something
        /// </summary>
        /// <value></value>
        public Salvage Salvage
        {
            get => MapObject as Salvage;
            set
            {
                MapObject = value;
            }
        }

        public override void _Ready()
        {
            base._Ready();
        }

        public override void _Draw()
        {
            if (!IsInstanceValid(this))
            {
                return;
            }
            DrawRect(new Rect2(-2, -2, 4, 4), Colors.Yellow, false, 1, true);
        }

        public override void UpdateSprite()
        {
            Update();
            // do nothing
        }
    }
}