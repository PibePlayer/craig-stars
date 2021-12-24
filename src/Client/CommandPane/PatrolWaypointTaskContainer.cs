using System;
using System.Collections.Generic;
using Godot;

namespace CraigStars.Client
{

    public class PatrolWaypointTaskContainer : VBoxContainer
    {
        public Waypoint Waypoint { get; set; } = new();

        OptionButton patrolRangeOptionButton;
        WarpFactor patrolWarpFactor;

        List<int> ranges = new();

        public override void _Ready()
        {
            base._Ready();
            patrolRangeOptionButton = GetNode<OptionButton>("GridContainer/PatrolRangeOptionButton");
            patrolWarpFactor = GetNode<WarpFactor>("GridContainer/PatrolWarpFactor");

            patrolRangeOptionButton.Connect("item_selected", this, nameof(OnPatrolRangeOptionButtonItemSelected));
            patrolWarpFactor.WarpSpeedChangedEvent += OnPatrolWarpFactorWarpSpeedChanged;

            ranges.Clear();
            for (int range = 50; range <= 550; range += 50)
            {
                patrolRangeOptionButton.AddItem($"within {range} l.y.", range);
                ranges.Add(range);
            }
            patrolRangeOptionButton.AddItem($"any enemy", Waypoint.PatrolRangeInfinite);
            ranges.Add(Waypoint.PatrolRangeInfinite);

            UpdateControls();
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                patrolWarpFactor.WarpSpeedChangedEvent -= OnPatrolWarpFactorWarpSpeedChanged;
            }
        }

        void OnPatrolWarpFactorWarpSpeedChanged(int warpFactor)
        {
            Waypoint.PatrolWarpFactor = warpFactor;
        }

        void OnPatrolRangeOptionButtonItemSelected(int index)
        {
            Waypoint.PatrolRange = ranges[index];
        }

        void UpdateControls()
        {
            if (patrolRangeOptionButton != null && Waypoint != null)
            {
                patrolRangeOptionButton.Selected = ranges.FindIndex(range => range == Waypoint.PatrolRange);
                patrolWarpFactor.WarpSpeed = Waypoint.PatrolWarpFactor == Waypoint.PatrolRangeInfinite ? 0 : Waypoint.PatrolWarpFactor;
            }
        }
    }
}