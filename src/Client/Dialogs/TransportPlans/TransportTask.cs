using Godot;
using System;
using CraigStars.Utils;

namespace CraigStars
{
    /// <summary>
    /// Allows control over a WaypointTransportTask changing fields based on CargoType and action
    /// </summary>
    [Tool]
    public class TransportTask : Container
    {
        public event Action<CargoType, WaypointTaskTransportAction, int> TransportTaskUpdatedEvent;

        [Export]
        public CargoType CargoType { get; set; }

        public WaypointTaskTransportAction Action { get; set; }
        public int Amount { get; set; } = 0;

        Label cargoTypeLabel;
        OptionButton waypointTaskTransportActionOptionButton;
        SpinBox amountSpinBox;

        public override void _Ready()
        {
            cargoTypeLabel = GetNode<Label>("CargoTypeLabel");
            waypointTaskTransportActionOptionButton = GetNode<OptionButton>("WaypointTaskTransportActionOptionButton");
            amountSpinBox = GetNode<SpinBox>("AmountSpinBox");

            waypointTaskTransportActionOptionButton.PopulateOptionButton<WaypointTaskTransportAction>((action) => EnumUtils.GetLabelForWaypointTaskTransportAction(action));

            waypointTaskTransportActionOptionButton.Connect("item_selected", this, nameof(OnWaypointTaskTransportActionOptionButtonItemSelected));
            amountSpinBox.Connect("value_changed", this, nameof(OnAmountSpinBoxValueChanged));

            cargoTypeLabel.Text = CargoType.ToString();
            switch (CargoType)
            {
                case CargoType.Ironium:
                    cargoTypeLabel.Modulate = new Color("120afe");
                    break;
                case CargoType.Boranium:
                    cargoTypeLabel.Modulate = new Color("08810a");
                    break;
                case CargoType.Germanium:
                    cargoTypeLabel.Modulate = new Color("feff00");
                    break;
                case CargoType.Colonists:
                    cargoTypeLabel.Modulate = Colors.White;
                    break;
                case CargoType.Fuel:
                    cargoTypeLabel.Modulate = Colors.Red;
                    break;
                default:
                    cargoTypeLabel.Modulate = Colors.White;
                    break;
            }

            UpdateControls();
        }

        void OnAmountSpinBoxValueChanged(float value)
        {
            Amount = (int)value;
            TransportTaskUpdatedEvent?.Invoke(CargoType, Action, Amount);
        }

        void OnWaypointTaskTransportActionOptionButtonItemSelected(int index)
        {
            Action = (WaypointTaskTransportAction)index;
            UpdateAmountSpinBoxProperties();
            TransportTaskUpdatedEvent?.Invoke(CargoType, Action, Amount);
        }

        void UpdateAmountSpinBoxProperties()
        {
            amountSpinBox.Suffix = "";
            switch (Action)
            {
                case WaypointTaskTransportAction.LoadAll:
                case WaypointTaskTransportAction.UnloadAll:
                    amountSpinBox.Editable = false;
                    break;
                case WaypointTaskTransportAction.LoadAmount:
                case WaypointTaskTransportAction.LoadDunnage:
                case WaypointTaskTransportAction.UnloadAmount:
                case WaypointTaskTransportAction.SetAmountTo:
                case WaypointTaskTransportAction.SetWaypointTo:
                    amountSpinBox.MaxValue = int.MaxValue;
                    amountSpinBox.Editable = true;
                    amountSpinBox.Suffix = "kT";
                    break;

                case WaypointTaskTransportAction.FillPercent:
                case WaypointTaskTransportAction.WaitForPercent:
                    amountSpinBox.MaxValue = 100;
                    amountSpinBox.Editable = true;
                    amountSpinBox.Suffix = "%";
                    break;
                default:
                    amountSpinBox.Editable = false;
                    break;
            }
        }

        public void UpdateControls()
        {
            waypointTaskTransportActionOptionButton.Selected = (int)Action;
            amountSpinBox.Value = Amount;
            UpdateAmountSpinBoxProperties();
        }

    }
}