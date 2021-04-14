using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    public class TransportPlanDetail : VBoxContainer
    {
        public TransportPlan Plan
        {
            get => plan;
            set
            {
                plan = value;
                UpdateControls();
            }
        }
        TransportPlan plan = new TransportPlan();

        [Export]
        public bool ShowName { get; set; } = true;

        Control nameContainer;
        LineEdit nameLineEdit;
        TransportTask ironiumTransportTask;
        TransportTask boraniumTransportTask;
        TransportTask germaniumTransportTask;
        TransportTask colonistsTransportTask;
        TransportTask fuelTransportTask;

        public override void _Ready()
        {
            nameContainer = GetNode<Control>("NameContainer");
            nameLineEdit = GetNode<LineEdit>("NameContainer/NameLineEdit");

            ironiumTransportTask = GetNode<TransportTask>("IroniumTransportTask");
            boraniumTransportTask = GetNode<TransportTask>("BoraniumTransportTask");
            germaniumTransportTask = GetNode<TransportTask>("GermaniumTransportTask");
            colonistsTransportTask = GetNode<TransportTask>("ColonistsTransportTask");
            fuelTransportTask = GetNode<TransportTask>("FuelTransportTask");

            nameLineEdit.Connect("text_changed", this, nameof(OnNameLineEditTextChanged));

            ironiumTransportTask.TransportTaskUpdatedEvent += OnTransportTaskUpdated;
            boraniumTransportTask.TransportTaskUpdatedEvent += OnTransportTaskUpdated;
            germaniumTransportTask.TransportTaskUpdatedEvent += OnTransportTaskUpdated;
            colonistsTransportTask.TransportTaskUpdatedEvent += OnTransportTaskUpdated;
            fuelTransportTask.TransportTaskUpdatedEvent += OnTransportTaskUpdated;
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            ironiumTransportTask.TransportTaskUpdatedEvent -= OnTransportTaskUpdated;
            boraniumTransportTask.TransportTaskUpdatedEvent -= OnTransportTaskUpdated;
            germaniumTransportTask.TransportTaskUpdatedEvent -= OnTransportTaskUpdated;
            colonistsTransportTask.TransportTaskUpdatedEvent -= OnTransportTaskUpdated;
            fuelTransportTask.TransportTaskUpdatedEvent -= OnTransportTaskUpdated;
        }

        private void OnTransportTaskUpdated(CargoType cargoType, WaypointTaskTransportAction action, int amount)
        {
            var tasks = Plan.Tasks;
            tasks[cargoType] = new WaypointTransportTask(action, amount);
            Plan.Tasks = tasks;
        }

        void OnNameLineEditTextChanged(string newText)
        {
            if (Plan != null)
            {
                Plan.Name = newText;
            }
        }

        public void UpdateControls()
        {
            nameContainer.Visible = ShowName;

            if (Plan != null)
            {
                nameLineEdit.Text = Plan.Name;

                ironiumTransportTask.Action = Plan.Tasks.Ironium.action;
                boraniumTransportTask.Action = Plan.Tasks.Boranium.action;
                germaniumTransportTask.Action = Plan.Tasks.Germanium.action;
                colonistsTransportTask.Action = Plan.Tasks.Colonists.action;
                fuelTransportTask.Action = Plan.Tasks.Fuel.action;

                ironiumTransportTask.Amount = Plan.Tasks.Ironium.amount;
                boraniumTransportTask.Amount = Plan.Tasks.Boranium.amount;
                germaniumTransportTask.Amount = Plan.Tasks.Germanium.amount;
                colonistsTransportTask.Amount = Plan.Tasks.Colonists.amount;
                fuelTransportTask.Amount = Plan.Tasks.Fuel.amount;

                ironiumTransportTask.UpdateControls();
                boraniumTransportTask.UpdateControls();
                germaniumTransportTask.UpdateControls();
                colonistsTransportTask.UpdateControls();
                fuelTransportTask.UpdateControls();
            }
        }

    }
}