using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars.Client
{
    /// <summary>
    /// This represents the coverage of a scanner, either penetrating or regular
    /// We separate pen from regular because we draw all regular, and then all pen scanners
    /// </summary>
    public class ScannerCoverage : Node2D
    {
        protected Player Me { get => PlayersManager.Me; }

        public int ScanRange
        {
            get => scanRange;
            set
            {
                scanRange = value;
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

        public override void _Ready()
        {
            base._Ready();
            Update();

            EventManager.ScannerScaleUpdatedEvent += OnScannerScaleUpdated;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                EventManager.ScannerScaleUpdatedEvent -= OnScannerScaleUpdated;
            }
        }

        public override void _Draw()
        {
            int scaledRange = (int)(ScanRange * Me.UISettings.ScannerPercent / 100f);
            if (Pen)
            {
                if (scaledRange > 0)
                {
                    DrawCircle(Vector2.Zero, scaledRange, GUIColors.ScannerPenColor);
                }
            }
            else
            {
                if (scaledRange > 0)
                {
                    DrawCircle(Vector2.Zero, scaledRange, GUIColors.ScannerColor);
                }
            }
        }

        void OnScannerScaleUpdated()
        {
            if (IsInstanceValid(this))
            {
                Update();
            }
        }
    }
}