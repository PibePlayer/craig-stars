using System;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class ShipDesigner : HBoxContainer
    {
        [Inject] FleetSpecService fleetSpecService;
        public event Action CancelledEvent;
        protected Player Me { get => PlayersManager.Me; }

        /// <summary>
        /// This is the source ship design we use to populate the hull designer
        /// </summary>
        /// <returns></returns>
        public ShipDesign SourceShipDesign { get; set; }
        public TechHull Hull { get; set; }
        public bool EditingExisting { get; set; } = false;

        public bool IsValid { get; private set; }
        public bool IsDirty { get; private set; } = false;

        TechTree hullComponentsTechTree;
        CostGrid hullComponentCostGrid;
        Label hullComponentCostLabel;
        HullSummary designerHullSummary;
        Button saveDesignButton;
        Button cancelDesignButton;
        LineEdit designNameLineEdit;
        Label versionLabel;
        Label errorLabel;

        public override void _Ready()
        {
            this.ResolveDependencies();
            hullComponentsTechTree = FindNode("HullComponentsTechTree") as TechTree;
            hullComponentCostGrid = FindNode("HullComponentCostGrid") as CostGrid;
            hullComponentCostLabel = FindNode("HullComponentCostLabel") as Label;
            designerHullSummary = FindNode("DesignerHullSummary") as HullSummary;
            saveDesignButton = FindNode("SaveDesignButton") as Button;
            cancelDesignButton = FindNode("CancelDesignButton") as Button;
            designNameLineEdit = FindNode("DesignNameLineEdit") as LineEdit;
            versionLabel = FindNode("VersionLabel") as Label;
            errorLabel = FindNode("ErrorLabel") as Label;

            designNameLineEdit.Connect("text_changed", this, nameof(OnDesignNameLineEditTextChanged));
            saveDesignButton.Connect("pressed", this, nameof(OnSaveDesignButtonPressed));
            cancelDesignButton.Connect("pressed", this, nameof(OnCancelDesignButtonPressed));
            hullComponentsTechTree.TechSelectedEvent += OnHullComponentSelectedEvent;

            designerHullSummary.SlotUpdatedEvent += OnSlotUpdated;
            designerHullSummary.SlotPressedEvent += OnSlotPressed;

            Connect("visibility_changed", this, nameof(OnVisible));
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                designerHullSummary.SlotUpdatedEvent -= OnSlotUpdated;
                designerHullSummary.SlotPressedEvent -= OnSlotPressed;
                hullComponentsTechTree.TechSelectedEvent -= OnHullComponentSelectedEvent;
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_save") && !saveDesignButton.Disabled && IsVisibleInTree())
            {
                // save the game if save is clicked
                OnSaveDesignButtonPressed();
                GetTree().SetInputAsHandled();
            }
        }

        void OnSlotPressed(HullComponentPanel hullComponentPanel, TechHullComponent hullComponent)
        {
            if (hullComponent != null)
            {
                hullComponentCostLabel.Text = $"Cost of one {hullComponent.Name}";
                hullComponentCostGrid.Cost = hullComponent.GetPlayerCost(Me);
            }
        }

        void OnSlotUpdated(ShipDesignSlot slot)
        {
            // revalidate
            IsDirty = true;
            saveDesignButton.Text = "Save Design";
            cancelDesignButton.Text = "Cancel";
            UpdateControls();
        }

        /// <summary>
        /// Update the Designer's ShipDesign with a clone of our SourceShipDesign
        /// </summary>
        void ResetDesignerShipDesignFromSource()
        {
            if (SourceShipDesign != null)
            {
                var design = SourceShipDesign.Copy();
                fleetSpecService.ComputeDesignSpec(PlayersManager.Me, design);
                if (!EditingExisting)
                {
                    design.Version++;
                }

                designerHullSummary.ShipDesign = design;
                designerHullSummary.UpdateControls();
                designNameLineEdit.Text = SourceShipDesign.Name != null ? SourceShipDesign.Name : "";
            }
        }

        void OnVisible()
        {
            if (IsVisibleInTree() && SourceShipDesign != null)
            {
                ResetDesignerShipDesignFromSource();
                designerHullSummary.Hull = Hull;
                designerHullSummary.UpdateControls();
                saveDesignButton.Text = "Save Design";
                cancelDesignButton.Text = "Cancel";
                versionLabel.Text = $"v{designerHullSummary.ShipDesign.Version}";
                IsDirty = false;
                UpdateControls();
            }
            else
            {
                // reset the designer ship design
                // from a clone
                ResetDesignerShipDesignFromSource();

                designNameLineEdit.Text = designerHullSummary.ShipDesign.Name;
                IsDirty = false;
                UpdateControls();
            }
        }

        void OnHullComponentSelectedEvent(Tech tech)
        {
            if (tech is TechHullComponent hullComponent)
            {
                hullComponentCostLabel.Text = $"Cost of one {hullComponent.Name}";
                hullComponentCostGrid.Cost = hullComponent.GetPlayerCost(Me);
            }
        }

        void OnDesignNameLineEditTextChanged(string newText)
        {
            IsDirty = true;
            saveDesignButton.Text = "Save Design";
            cancelDesignButton.Text = "Cancel";
            UpdateControls();
        }

        void OnSaveDesignButtonPressed()
        {
            // TODO, support updates
            designerHullSummary.ShipDesign.Name = designNameLineEdit.Text;
            fleetSpecService.ComputeDesignSpec(PlayersManager.Me, designerHullSummary.ShipDesign, true);
            if (EditingExisting)
            {
                designerHullSummary.ShipDesign.Status = ShipDesign.DesignStatus.Current;
                // remove the old design and add the new one
                var designs = PlayersManager.Me.Designs;
                int index = designs.FindIndex(sd => sd == SourceShipDesign);
                designs.RemoveAt(index);
                designs.Insert(index, designerHullSummary.ShipDesign);
            }
            else
            {
                designerHullSummary.ShipDesign.Status = ShipDesign.DesignStatus.New;
                PlayersManager.Me.Designs.Add(designerHullSummary.ShipDesign);
            }
            saveDesignButton.Text = "Saved";
            cancelDesignButton.Text = "Close";
            EditingExisting = true;
            IsDirty = false;

            // reset the design after save
            SourceShipDesign = designerHullSummary.ShipDesign;
            ResetDesignerShipDesignFromSource();
            UpdateControls();
            Me.Dirty = true;
            EventManager.PublishPlayerDirtyEvent();
        }

        void OnCancelDesignButtonPressed()
        {
            CancelledEvent?.Invoke();
        }

        void UpdateControls()
        {
            var name = designNameLineEdit.Text;
            var version = designerHullSummary.ShipDesign.Version;
            // Check if we have a ship design by this name already. If we 
            var nameAlreadyExists = PlayersManager.Me.Designs
                // If we are not editing existing, check all ship designs for name conflicts
                // if we ARE editing an existing design, remove it from the ships to check for name conflicts
                .Where(sd => !sd.Deleted)
                .Where(sd => !EditingExisting || sd != SourceShipDesign)
                .Any(sd => sd.Name == name && sd.Version == version);
            designNameLineEdit.Modulate = Colors.White;
            errorLabel.Visible = false;
            IsValid = true;
            saveDesignButton.Disabled = true;

            if (name.Trim() == "")
            {
                IsValid = false;
                designNameLineEdit.Modulate = Colors.Red;
            }
            else if (nameAlreadyExists)
            {
                IsValid = false;
                designNameLineEdit.Modulate = Colors.Red;
                errorLabel.Visible = true;
                errorLabel.Text = "Name must be unique";
            }

            if (!designerHullSummary.ShipDesign.IsValid())
            {
                IsValid = false;
            }

            if (IsValid)
            {
                saveDesignButton.Disabled = !IsDirty;
                designNameLineEdit.Modulate = Colors.White;
            }
        }
    }
}