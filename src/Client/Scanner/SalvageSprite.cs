using Godot;
using System;

namespace CraigStars.Client
{
    public class SalvageSprite : MapObjectSprite
    {
        /// <summary>
        /// Convenience method so the code looks like Fleet.Something instead of MapObject.Something
        /// </summary>
        /// <value></value>
        public Salvage Salvage
        {
            get => MapObject as Salvage;
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
            if (!IsInstanceValid(this))
            {
                return;
            }

            // do nothing
        }
    }
}