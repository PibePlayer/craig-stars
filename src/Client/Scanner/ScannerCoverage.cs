using System;
using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client
{
    /// <summary>
    /// This represents the coverage of a scanner, either penetrating or regular
    /// We separate pen from regular because we draw all regular, and then all pen scanners
    /// </summary>
    public class ScannerCoverage : Node2D, INodePoolNode
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

        public void Returned()
        {
            ScanRange = 0;
            Pen = false;
        }

        /// <summary>
        /// When the scanner is removed and added back to the tree, make sure
        /// the scanner images are updated
        /// </summary>
        public override void _EnterTree()
        {
            base._EnterTree();
            Update();
        }

        public override void _Draw()
        {
            int scaledRange = (int)(ScanRange * Me.UISettings.ScannerPercent / 100f);
            if (Pen)
            {
                if (scaledRange > 0)
                {
                    DrawCircle(Vector2.Zero, scaledRange, GUIColorsProvider.Colors.ScannerPenColor);
                }
            }
            else
            {
                if (scaledRange > 0)
                {
                    DrawCircle(Vector2.Zero, scaledRange, GUIColorsProvider.Colors.ScannerColor);
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