using Godot;
using System;

namespace CraigStars
{
    /// <summary>
    /// This represents the coverage of a scanner, either penetrating or regular
    /// We separate pen from regular because we draw all regular, and then all pen scanners
    /// </summary>
    public class ScannerCoverage : Node2D
    {
        public int ScanRange
        {
            get => scanRange;
            set
            {
                scanRange = value;
                Update();
            }
        }
        int scanRange = 0;

        /// <summary>
        /// Is this a penetrating scanner or regular?
        /// </summary>
        /// <value></value>
        public bool Pen { get; set; }

        [Export]
        public GUIColors GUIColors { get; set; } = new GUIColors();

        CollisionShape2D range;

        public override void _Ready()
        {
            range = FindNode("Range") as CollisionShape2D;
        }

        public override void _Draw()
        {
            if (Pen)
            {
                if (ScanRange > 0)
                {
                    DrawCircle(Vector2.Zero, ScanRange, GUIColors.ScannerPenColor);
                    if (range.Shape is CircleShape2D shape)
                    {
                        shape.Radius = ScanRange;
                    }
                }
            }
            else
            {
                if (ScanRange > 0)
                {
                    DrawCircle(Vector2.Zero, ScanRange, GUIColors.ScannerColor);
                    if (range.Shape is CircleShape2D shape)
                    {
                        shape.Radius = ScanRange;
                    }
                }

            }
        }
    }
}