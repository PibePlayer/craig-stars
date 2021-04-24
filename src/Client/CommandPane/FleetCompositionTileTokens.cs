using CraigStars.Singletons;
using Godot;
using System;
using System.Collections.Generic;

namespace CraigStars
{

    public class FleetCompositionTileTokens : AbstractCommandedFleetControl
    {
        PackedScene rowScene;

        List<FleetCompositionTileTokensRow> rows = new List<FleetCompositionTileTokensRow>();

        Control tokens;

        public override void _Ready()
        {
            base._Ready();

            rowScene = ResourceLoader.Load<PackedScene>("res://src/Client/CommandPane/FleetCompositionTileTokensRow.tscn");
            tokens = GetNode<Control>("ScrollContainer/Tokens");
        }

        protected override void OnFleetsCreated(List<Fleet> fleets)
        {
            ClearTokens();
            base.OnFleetsCreated(fleets);
        }

        protected override void OnFleetDeleted(FleetSprite fleet)
        {
            ClearTokens();
            base.OnFleetDeleted(fleet);
        }

        protected override void OnNewCommandedFleet()
        {
            ClearTokens();
        }

        protected override void UpdateControls()
        {
            if (CommandedFleet != null && rows.Count == 0)
            {
                CommandedFleet.Fleet.Tokens.ForEach(token =>
                {
                    var row = rowScene.Instance() as FleetCompositionTileTokensRow;
                    row.Token = token;
                    row.SelectedEvent += OnRowSelected;
                    rows.Add(row);
                    tokens.AddChild(row);
                });
            }
        }

        void OnRowSelected(FleetCompositionTileTokensRow row)
        {
            foreach (var otherRow in rows)
            {
                if (otherRow != row)
                {
                    otherRow.Selected = false;
                }
            }

            HullSummaryPopup.Instance.Hull = row.Token.Design.Hull;
            HullSummaryPopup.Instance.ShipDesign = row.Token.Design;
            HullSummaryPopup.Instance.Token = row.Token;
            HullSummaryPopup.Instance.PopupCentered();
        }

        void ClearTokens()
        {
            // on a new fleet, clear out the rows controls
            rows.ForEach(row =>
            {
                if (IsInstanceValid(row))
                {
                    row.SelectedEvent -= OnRowSelected;
                    row.GetParent().RemoveChild(row);
                    row.QueueFree();
                }
            });
            rows.Clear();

        }
    }
}