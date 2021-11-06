using Godot;
using System;

namespace CraigStars.Client
{

    public class MineralPacketSprite : MapObjectSprite
    {
        [Export]
        public GUIColors GUIColors { get; set; } = new GUIColors();

        /// <summary>
        /// Convenience method so the code looks like MineralPacket.Something instead of MapObject.Something
        /// </summary>
        /// <value></value>
        public MineralPacket MineralPacket
        {
            get => MapObject as MineralPacket;
        }

        public override void _Draw()
        {
            if (!IsInstanceValid(this))
            {
                return;
            }
            var color = Colors.Green;
            if (!OwnedByMe)
            {
                color = PlayerColor;
            }
            DrawRect(new Rect2(-5.5f, -5.5f, 11, 11), Colors.Green, false, 1, true);

            // for fleets owned by other players, draw a helpful line showing how fast the fleet is going
            // and where it will end up
            if (State == ScannerState.Selected)
            {
                var distancePerYear = MineralPacket.WarpFactor * MineralPacket.WarpFactor;
                var perpendicular = MineralPacket.Heading.Perpendicular();
                for (var i = 0; i < 5; i++)
                {
                    DrawLine(MineralPacket.Heading * i * distancePerYear, MineralPacket.Heading * ((i + 1) * distancePerYear), color, 2);
                    DrawLine(-MineralPacket.Heading * i * distancePerYear, -MineralPacket.Heading * ((i + 1) * distancePerYear), color, 2);

                    // draw wings per each warp
                    DrawLine(MineralPacket.Heading * i * distancePerYear - perpendicular * 5, MineralPacket.Heading * i * distancePerYear + perpendicular * 5, color, 2);
                    DrawLine(-MineralPacket.Heading * i * distancePerYear - perpendicular * 5, -MineralPacket.Heading * i * distancePerYear + perpendicular * 5, color, 2);
                }
            }

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