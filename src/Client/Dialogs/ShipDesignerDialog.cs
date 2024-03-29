using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class ShipDesignerDialog : GameViewDialog
    {
        [Inject] protected PlayerTechService playerTechService;

        TabContainer tabContainer;

        DesignTree shipDesignTree;
        HullSummary shipHullSummary;
        Button copyDesignButton;
        Button editDesignButton;
        Button deleteDesignButton;
        Button copyStarbaseDesignButton;
        Button editStarbaseDesignButton;
        Button deleteStarbaseDesignButton;
        Button createShipDesignButton;

        DesignTree starbaseDesignTree;
        HullSummary starbaseHullSummary;

        TechTree hullsTechTree;
        HullSummary hullHullSummary;

        Container shipDesignTabsContainer;
        ShipDesigner shipDesigner;

        public override void _Ready()
        {
            this.ResolveDependencies();
            base._Ready();

            shipDesignTabsContainer = FindNode("ShipDesignTabsContainer") as Container;
            tabContainer = FindNode("TabContainer") as TabContainer;

            // ships tab
            shipDesignTree = FindNode("ShipDesignTree") as DesignTree;
            shipHullSummary = FindNode("ShipHullSummary") as HullSummary;
            copyDesignButton = FindNode("CopyDesignButton") as Button;
            editDesignButton = FindNode("EditDesignButton") as Button;
            deleteDesignButton = FindNode("DeleteDesignButton") as Button;

            // starbases tab
            starbaseDesignTree = FindNode("StarbaseDesignTree") as DesignTree;
            starbaseHullSummary = FindNode("StarbaseHullSummary") as HullSummary;
            copyStarbaseDesignButton = FindNode("CopyStarbaseDesignButton") as Button;
            editStarbaseDesignButton = FindNode("EditStarbaseDesignButton") as Button;
            deleteStarbaseDesignButton = FindNode("DeleteStarbaseDesignButton") as Button;

            // hulls tab
            hullsTechTree = FindNode("HullsTechTree") as TechTree;
            hullHullSummary = FindNode("HullHullSummary") as HullSummary;
            createShipDesignButton = FindNode("CreateShipDesignButton") as Button;

            // ship designer control
            shipDesigner = FindNode("ShipDesigner") as ShipDesigner;
            shipDesigner.CancelledEvent += OnShipDesignerCancelled;

            shipDesignTree.DesignSelectedEvent += OnShipDesignSelectedEvent;
            starbaseDesignTree.DesignSelectedEvent += OnStarbaseDesignSelectedEvent;
            hullsTechTree.TechSelectedEvent += OnHullSelectedEvent;

            // wire up events for the various create/edit/delete buttons
            copyDesignButton.Connect("pressed", this, nameof(OnCopyDesignButtonPressed));
            editDesignButton.Connect("pressed", this, nameof(OnEditDesignButtonPressed));
            deleteDesignButton.Connect("pressed", this, nameof(OnDeleteShipDesignButtonPressed));
            copyStarbaseDesignButton.Connect("pressed", this, nameof(OnCopyStarbaseDesignButtonPressed));
            editStarbaseDesignButton.Connect("pressed", this, nameof(OnEditStarbaseDesignButtonPressed));
            deleteStarbaseDesignButton.Connect("pressed", this, nameof(OnDeleteStarbaseDesignButtonPressed));
            createShipDesignButton.Connect("pressed", this, nameof(OnCreateShipDesignButtonPressed));
            editDesignButton.Disabled = editStarbaseDesignButton.Disabled = true; // we can only edit designs that are not in use

        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                shipDesigner.CancelledEvent -= OnShipDesignerCancelled;
                shipDesignTree.DesignSelectedEvent -= OnShipDesignSelectedEvent;
                starbaseDesignTree.DesignSelectedEvent -= OnStarbaseDesignSelectedEvent;
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel") && IsVisibleInTree())
            {
                if (shipDesigner.Visible)
                {
                    if (shipDesigner.IsDirty)
                    {
                        CSConfirmDialog.Show("You have made changes to this design. Are you sure you want to close the designer?",
                        () =>
                        {
                            shipDesigner.Visible = false;
                            shipDesignTabsContainer.Visible = true;
                        });
                    }
                    else
                    {
                        shipDesigner.Visible = false;
                        shipDesignTabsContainer.Visible = true;
                    }
                    // cancel the popup
                    GetTree().SetInputAsHandled();
                }
            }
        }

        void OnShipDesignSelectedEvent(ShipDesign design)
        {
            shipHullSummary.ShipDesign = design;
            shipHullSummary.Hull = design.Hull;
            shipHullSummary.UpdateControls();
            editDesignButton.Disabled = design.InUse;
        }

        void OnStarbaseDesignSelectedEvent(ShipDesign design)
        {
            starbaseHullSummary.ShipDesign = design;
            starbaseHullSummary.Hull = design.Hull;
            starbaseHullSummary.UpdateControls();
            deleteStarbaseDesignButton.Disabled = !design.CanDelete;
            editStarbaseDesignButton.Disabled = design.InUse;
        }

        void OnHullSelectedEvent(Tech tech)
        {
            if (tech is TechHull hull)
            {
                hullHullSummary.Hull = hull;
                hullHullSummary.UpdateControls();
                createShipDesignButton.Disabled = !playerTechService.HasTech(Me, tech);
            }
        }

        void OnCopyDesignButtonPressed()
        {
            shipDesigner.EditingExisting = false;
            shipDesigner.Hull = shipHullSummary.Hull;
            shipDesigner.SourceShipDesign = shipHullSummary.ShipDesign;

            shipDesigner.Visible = true;
            shipDesignTabsContainer.Visible = false;
        }

        void OnEditDesignButtonPressed()
        {
            shipDesigner.EditingExisting = true;
            shipDesigner.Hull = shipHullSummary.Hull;
            shipDesigner.SourceShipDesign = shipHullSummary.ShipDesign;

            shipDesigner.Visible = true;
            shipDesignTabsContainer.Visible = false;
        }

        void OnCopyStarbaseDesignButtonPressed()
        {
            shipDesigner.EditingExisting = false;
            shipDesigner.Hull = starbaseHullSummary.Hull;
            shipDesigner.SourceShipDesign = starbaseHullSummary.ShipDesign;

            shipDesigner.Visible = true;
            shipDesignTabsContainer.Visible = false;
        }

        void OnEditStarbaseDesignButtonPressed()
        {
            shipDesigner.EditingExisting = true;
            shipDesigner.Hull = starbaseHullSummary.Hull;
            shipDesigner.SourceShipDesign = starbaseHullSummary.ShipDesign;

            shipDesigner.Visible = true;
            shipDesignTabsContainer.Visible = false;
        }

        void OnDeleteShipDesignButtonPressed()
        {
            OnDeleteDesignButtonPressed(shipHullSummary.ShipDesign, () => { shipDesignTree.UpdateTreeItems(); shipDesignTree.SelectFirstItem(); });
        }

        void OnDeleteStarbaseDesignButtonPressed()
        {
            OnDeleteDesignButtonPressed(starbaseHullSummary.ShipDesign, () => { starbaseDesignTree.UpdateTreeItems(); starbaseDesignTree.SelectFirstItem(); });
        }

        void OnDeleteDesignButtonPressed(ShipDesign designToDelete, Action deletedCallback)
        {
            var designIndex = Me.Designs.FindIndex(d => d == designToDelete);
            if (designIndex != -1)
            {
                var design = Me.Designs[designIndex];

                var message = $"Are you sure you want to delete the design {design.Name}?";
                if (design.InUse)
                {
                    message = $"{design.Name} is in use. All fleet tokens with this design will be immediately deleted. Are you sure you want to delete the design {design.Name}?";
                }
                // TODO: handle deleting designs with existing fleets.
                CSConfirmDialog.Show(
                    message,
                    () =>
                    {

                        Me.DeleteDesign(design);

                        // delete any fleets with no tokens left
                        var fleetsToDelete = new List<Fleet>();
                        foreach (var fleet in Me.Fleets.Where(_ => _.Tokens.Count == 0))
                        {
                            fleetsToDelete.Add(fleet);
                            EventManager.PublishFleetDeletedEvent(fleet);
                        }
                        fleetsToDelete.ForEach(fleetToDelete => Me.Fleets.Remove(fleetToDelete));

                        deletedCallback();
                    }
                );
            }
        }

        void OnCreateShipDesignButtonPressed()
        {
            shipDesigner.EditingExisting = false;
            shipDesigner.Hull = hullHullSummary.Hull;
            shipDesigner.SourceShipDesign = new ShipDesign() { PlayerNum = Me.Num, Hull = shipDesigner.Hull };

            shipDesigner.Visible = true;
            shipDesignTabsContainer.Visible = false;
        }

        void OnShipDesignerCancelled()
        {
            if (shipDesigner.IsDirty)
            {
                CSConfirmDialog.Show("You have made changes to this design. Are you sure you want to close the designer?",
                () =>
                {
                    shipDesigner.Visible = false;
                    shipDesignTabsContainer.Visible = true;
                });
            }
            else
            {
                shipDesigner.Visible = false;
                shipDesignTabsContainer.Visible = true;
            }
        }

    }
}