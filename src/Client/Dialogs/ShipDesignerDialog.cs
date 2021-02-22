using CraigStars.Singletons;
using Godot;
using System;
using System.Threading.Tasks;

namespace CraigStars
{
    public class ShipDesignerDialog : WindowDialog
    {

        Button okButton;
        Player me;

        CSConfirmDialog confirmationDialog;

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

        ShipDesigner shipDesigner;
        Label noDesignLabel;

        public override void _Ready()
        {
            me = PlayersManager.Instance.Me;
            okButton = FindNode("OKButton") as Button;
            tabContainer = FindNode("TabContainer") as TabContainer;
            confirmationDialog = FindNode("ConfirmationDialog") as CSConfirmDialog;

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

            // ship designer tab
            shipDesigner = FindNode("ShipDesigner") as ShipDesigner;
            noDesignLabel = FindNode("NoDesignLabel") as Label;

            shipDesignTree.DesignSelectedEvent += OnShipDesignSelectedEvent;
            starbaseDesignTree.DesignSelectedEvent += OnStarbaseDesignSelectedEvent;
            hullsTechTree.TechSelectedEvent += OnHullSelectedEvent;

            Connect("about_to_show", this, nameof(OnAboutToShow));
            Connect("popup_hide", this, nameof(OnPopupHide));
            okButton.Connect("pressed", this, nameof(OnOk));

            // wire up events for the various create/edit/delete buttons
            copyDesignButton.Connect("pressed", this, nameof(OnCopyDesignButtonPressed));
            editDesignButton.Connect("pressed", this, nameof(OnEditDesignButtonPressed));
            deleteDesignButton.Connect("pressed", this, nameof(OnDeleteDesignButtonPressed));
            copyStarbaseDesignButton.Connect("pressed", this, nameof(OnCopyStarbaseDesignButtonPressed));
            editStarbaseDesignButton.Connect("pressed", this, nameof(OnEditStarbaseDesignButtonPressed));
            deleteStarbaseDesignButton.Connect("pressed", this, nameof(OnDeleteStarbaseDesignButtonPressed));
            createShipDesignButton.Connect("pressed", this, nameof(OnCreateShipDesignButtonPressed));
            editDesignButton.Disabled = editStarbaseDesignButton.Disabled = true; // we can only edit designs that are not in use
        }

        public override void _ExitTree()
        {
            shipDesignTree.DesignSelectedEvent -= OnShipDesignSelectedEvent;
            starbaseDesignTree.DesignSelectedEvent -= OnStarbaseDesignSelectedEvent;
        }

        void OnShipDesignSelectedEvent(ShipDesign design)
        {
            shipHullSummary.ShipDesign = design;
            shipHullSummary.Hull = design.Hull;
            editDesignButton.Disabled = design.Aggregate.InUse;
        }

        void OnStarbaseDesignSelectedEvent(ShipDesign design)
        {
            starbaseHullSummary.ShipDesign = design;
            starbaseHullSummary.Hull = design.Hull;
            editStarbaseDesignButton.Disabled = design.Aggregate.InUse;
        }

        void OnHullSelectedEvent(Tech tech)
        {
            if (tech is TechHull hull)
            {
                hullHullSummary.Hull = hull;
            }
        }


        /// <summary>
        /// Hide show
        /// </summary>
        void OnAboutToShow()
        {
            var design = new ShipDesign { Hull = Techs.Scout, Player = me };
            shipDesigner.Hull = Techs.Scout;
        }

        /// <summary>
        /// Called when the popup hides
        /// </summary>
        void OnPopupHide()
        {
        }

        /// <summary>
        /// Just hide the dialog on ok
        /// </summary>
        void OnOk()
        {
            Hide();
        }

        void OnCopyDesignButtonPressed()
        {
            noDesignLabel.Visible = false;
            shipDesigner.EditingExisting = false;
            shipDesigner.Hull = shipHullSummary.Hull;
            shipDesigner.SourceShipDesign = shipHullSummary.ShipDesign;
            shipDesigner.Visible = true;

            tabContainer.CurrentTab = 3;
        }

        void OnEditDesignButtonPressed()
        {
            noDesignLabel.Visible = false;
            shipDesigner.EditingExisting = true;
            shipDesigner.Hull = shipHullSummary.Hull;
            shipDesigner.SourceShipDesign = shipHullSummary.ShipDesign;
            shipDesigner.Visible = true;
            tabContainer.CurrentTab = 3;
        }

        void OnDeleteDesignButtonPressed()
        {
            // TODO: handle deleting designs with existing fleets.
            confirmationDialog.Show(
                $"Are you sure you want to delete the design {shipHullSummary.ShipDesign.Name}?",
                () =>
                {
                    var designIndex = me.Designs.FindIndex(d => d == shipHullSummary.ShipDesign);
                    me.DeletedDesigns.Add(me.Designs[designIndex]);
                    me.Designs.RemoveAt(designIndex);
                    shipDesignTree.UpdateTreeItems();
                }
            );
        }

        void OnCopyStarbaseDesignButtonPressed()
        {
            noDesignLabel.Visible = false;
            shipDesigner.EditingExisting = false;
            shipDesigner.Hull = starbaseHullSummary.Hull;
            shipDesigner.SourceShipDesign = starbaseHullSummary.ShipDesign;
            shipDesigner.Visible = true;
            tabContainer.CurrentTab = 3;
        }

        void OnEditStarbaseDesignButtonPressed()
        {
            noDesignLabel.Visible = false;
            shipDesigner.EditingExisting = true;
            shipDesigner.Hull = starbaseHullSummary.Hull;
            shipDesigner.SourceShipDesign = starbaseHullSummary.ShipDesign;
            shipDesigner.Visible = true;
            tabContainer.CurrentTab = 3;
        }

        void OnDeleteStarbaseDesignButtonPressed()
        {

        }

        void OnCreateShipDesignButtonPressed()
        {
            noDesignLabel.Visible = false;
            shipDesigner.EditingExisting = false;
            shipDesigner.Hull = hullHullSummary.Hull;
            shipDesigner.SourceShipDesign = new ShipDesign() { Hull = shipDesigner.Hull };
            shipDesigner.Visible = true;

            tabContainer.CurrentTab = 3;
        }

    }
}