using System;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class WormholeSummaryContainer : MapObjectSummary<WormholeSprite>
    {
        static CSLog log = LogProvider.GetLogger(typeof(WormholeSummaryContainer));

        TextureRect icon;
        Label location;
        Label destination;
        Label stability;

        public override void _Ready()
        {
            base._Ready();
            location = GetNode<Label>("HBoxContainer/GridContainer/Location");
            destination = GetNode<Label>("HBoxContainer/GridContainer/Destination");
            stability = GetNode<Label>("HBoxContainer/GridContainer/Stability");
            icon = GetNode<TextureRect>("HBoxContainer/Panel/Icon");
        }

        protected override void UpdateControls()
        {
            if (MapObject != null)
            {
                var wormhole = MapObject.Wormhole;
                location.Text = $"{TextUtils.GetPositionString(wormhole.Position)}";
                if (wormhole.Destination != null)
                {
                    destination.Text = $"{TextUtils.GetPositionString(wormhole.Destination.Position)}";
                }
                else
                {
                    destination.Text = "Unknown";
                }
                stability.Text = $"{EnumUtils.GetLabelForWormholeStability(wormhole.Stability)}";
            }
        }
    }
}