using System;
using Godot;

namespace CraigStars.Client
{
    public class WormholeSprite : MapObjectSprite
    {
        /// <summary>
        /// Convenience method so the code looks like Fleet.Something instead of MapObject.Something
        /// </summary>
        /// <value></value>
        public Wormhole Wormhole
        {
            get => MapObject as Wormhole;
        }

        Line2D desinationLine;

        public override void _Ready()
        {
            base._Ready();

            desinationLine = GetNode<Line2D>("DestinationLine");
        }

        public override void UpdateSprite()
        {
            if (!IsInstanceValid(this) || desinationLine == null)
            {
                return;
            }

            if (Wormhole != null && Wormhole.Destination != null)
            {
                desinationLine.Points = new Vector2[] {
                    new Vector2(),
                    Wormhole.Destination.Position - Wormhole.Position
                };
            }
            else
            {
                desinationLine.Points = new Vector2[] { Position };
            }
        }
    }
}