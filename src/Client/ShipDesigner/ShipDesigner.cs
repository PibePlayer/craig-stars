using CraigStars.Singletons;
using Godot;
using System;
using System.Linq;

namespace CraigStars
{
    public class ShipDesigner : HBoxContainer
    {
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
        HullSummary designerHullSummary;
        Button saveDesignButton;
        Button cancelDesignButton;
        LineEdit designNameLineEdit;
        Label errorLabel;

        public override void _Ready()
        {
            hullComponentsTechTree = FindNode("HullComponentsTechTree") as TechTree;
            designerHullSummary = FindNode("DesignerHullSummary") as HullSummary;
            saveDesignButton = FindNode("SaveDesignButton") as Button;
            cancelDesignButton = FindNode("CancelDesignButton") as Button;
            designNameLineEdit = FindNode("DesignNameLineEdit") as LineEdit;
            errorLabel = FindNode("ErrorLabel") as Label;

            designNameLineEdit.Connect("text_changed", this, nameof(OnDesignNameLineEditTextChanged));
            saveDesignButton.Connect("pressed", this, nameof(OnSaveDesignButtonPressed));
            cancelDesignButton.Connect("pressed", this, nameof(OnCancelDesignButtonPressed));
            hullComponentsTechTree.TechSelectedEvent += OnHullComponentSelectedEvent;

            designerHullSummary.SlotUpdatedEvent += OnSlotUpdated;

            Connect("visibility_changed", this, nameof(OnVisible));
        }

        public override void _ExitTree()
        {
            designerHullSummary.SlotUpdatedEvent -= OnSlotUpdated;
            hullComponentsTechTree.TechSelectedEvent -= OnHullComponentSelectedEvent;
        }

        void OnSlotUpdated(ShipDesignSlot slot)
        {
            // revalidate
            IsDirty = true;
            saveDesignButton.Text = "Save Design";
            UpdateControls();
        }

        /// <summary>
        /// Update the Designer's ShipDesign with a clone of our SourceShipDesign
        /// </summary>
        void ResetDesignerShipDesignFromSource()
        {
            var design = SourceShipDesign.Copy();
            design.ComputeAggregate(PlayersManager.Me);

            designerHullSummary.ShipDesign = design;

        }

        void OnVisible()
        {
            if (Visible)
            {
                ResetDesignerShipDesignFromSource();
                designerHullSummary.Hull = Hull;
                saveDesignButton.Text = "Save Design";
                IsDirty = false;
                UpdateControls();
            }
        }

        void OnHullComponentSelectedEvent(Tech tech)
        {
            if (tech is TechHullComponent hullComponent)
            {
                // todo do something with the designer
            }
        }

        void OnDesignNameLineEditTextChanged(string newText)
        {
            IsDirty = true;
            saveDesignButton.Text = "Save Design";
            UpdateControls();
        }

        void OnSaveDesignButtonPressed()
        {
            // TODO, support updates
            designerHullSummary.ShipDesign.Name = designNameLineEdit.Text;
            designerHullSummary.ShipDesign.ComputeAggregate(PlayersManager.Me);
            if (EditingExisting)
            {
                // remove the old design and add the new one
                var designs = PlayersManager.Me.Designs;
                int index = designs.FindIndex(sd => sd == SourceShipDesign);
                designs.RemoveAt(index);
                designs.Insert(index, designerHullSummary.ShipDesign);
            }
            else
            {
                PlayersManager.Me.Designs.Add(designerHullSummary.ShipDesign);
            }
            saveDesignButton.Text = "Saved";
            EditingExisting = true;
            IsDirty = false;

            // reset the design after save
            SourceShipDesign = designerHullSummary.ShipDesign;
            ResetDesignerShipDesignFromSource();
            UpdateControls();
        }

        void OnCancelDesignButtonPressed()
        {
            // reset the designer ship design
            // from a clone
            ResetDesignerShipDesignFromSource();

            designNameLineEdit.Text = designerHullSummary.ShipDesign.Name;
            IsDirty = false;
            UpdateControls();
        }

        void UpdateControls()
        {
            var name = designNameLineEdit.Text;
            // Check if we have a ship design by this name already. If we 
            var nameAlreadyExists = PlayersManager.Me.Designs
                // If we are not editing existing, check all ship designs for name conflicts
                // if we ARE editing an existing design, remove it from the ships to check for name conflicts
                .Where(sd => !EditingExisting || sd != SourceShipDesign)
                .Any(sd => sd.Name == name);
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