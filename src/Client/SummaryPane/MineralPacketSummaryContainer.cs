using Godot;
using System;
using CraigStars.Singletons;
using log4net;

namespace CraigStars
{
    public class MineralPacketSummaryContainer : MapObjectSummary<MineralPacketSprite>
    {
        static ILog log = LogManager.GetLogger(typeof(MineralPacketSummaryContainer));

        TextureRect icon;
        Label mineralpacketRaceLabel;
        Label warpFactorLabel;
        Control destinationContainer;
        Label destination;
        CargoGrid cargoGrid;

        public override void _Ready()
        {
            base._Ready();
            icon = GetNode<TextureRect>("VBoxContainer/HBoxContainer/IconContainer/Icon");
            mineralpacketRaceLabel = GetNode<Label>("VBoxContainer/HBoxContainer/IconContainer/RaceContainer/RaceLabel");
            warpFactorLabel = GetNode<Label>("VBoxContainer/HBoxContainer/VBoxContainer/WarpFactorLabel");
            destinationContainer = GetNode<Control>("VBoxContainer/HBoxContainer/VBoxContainer/DestinationContainer");
            destination = GetNode<Label>("VBoxContainer/HBoxContainer/VBoxContainer/DestinationContainer/Destination");
            cargoGrid = GetNode<CargoGrid>("VBoxContainer/HBoxContainer/VBoxContainer/CargoGrid");
        }

        protected override void UpdateControls()
        {

            if (MapObject != null)
            {
                var race = Me.Race;
                var mineralpacket = MapObject.MineralPacket;

                mineralpacketRaceLabel.Text = $"{mineralpacket.Owner.RacePluralName}";

                if (MapObject.OwnedByMe)
                {
                    destinationContainer.Visible = true;
                    destination.Text = $"{mineralpacket.Target.Name}";
                }
                else
                {
                    destinationContainer.Visible = true;
                }

                warpFactorLabel.Text = $"Traveling at Warp {mineralpacket.WarpFactor}";
                cargoGrid.Cargo = mineralpacket.Cargo;
            }
        }
    }
}