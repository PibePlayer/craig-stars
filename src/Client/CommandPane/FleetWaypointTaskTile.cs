using System;
using System.Collections.Generic;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class FleetWaypointTaskTile : FleetWaypointTile
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetWaypointTaskTile));

        enum LayMineFieldDuration
        {
            Indefinitely,
            One,
            Two,
            Three,
            Four,
            Five,
        }

        OptionButton waypointTask;
        Control transportContainer;
        TextureRect fuelTask;
        TextureRect ironiumTask;
        TextureRect boraniumTask;
        TextureRect germaniumTask;
        TextureRect colonistsTask;
        Button editButton;
        MenuButton applyPlanMenuButton;
        PopupPanel transportPlanEditPopupPanel;
        Button transportPlanEditOKButton;
        TransportPlanDetail transportPlanDetail;

        RemoteMiningWaypointTaskContainer remoteMiningWaypointTaskContainer;
        PatrolWaypointTaskContainer patrolWaypointTaskContainer;
        TransferFleetWaypointTaskContainer transferFleetWaypointTaskContainer;

        Control layMineFieldWaypointTaksContainer;
        OptionButton layMineFieldDurationOptionButton;
        Label minesLaidPerYearLabel;

        Texture loadTexture;
        Texture unloadTexture;
        Texture noneTexture;

        public override void _Ready()
        {
            base._Ready();

            loadTexture = CSResourceLoader.GetTexture("ArrowUp.svg");
            unloadTexture = CSResourceLoader.GetTexture("ArrowDown.svg");
            noneTexture = CSResourceLoader.GetTexture("Close.svg");

            waypointTask = (OptionButton)FindNode("WaypointTask");

            transportContainer = (Container)FindNode("TransportContainer");
            fuelTask = (TextureRect)FindNode("FuelTask");
            ironiumTask = (TextureRect)FindNode("IroniumTask");
            boraniumTask = (TextureRect)FindNode("BoraniumTask");
            germaniumTask = (TextureRect)FindNode("GermaniumTask");
            colonistsTask = (TextureRect)FindNode("ColonistsTask");
            editButton = (Button)FindNode("EditButton");
            applyPlanMenuButton = (MenuButton)FindNode("ApplyPlanMenuButton");
            transportPlanEditPopupPanel = (PopupPanel)FindNode("TransportPlanEditPopupPanel");
            transportPlanEditOKButton = (Button)FindNode("TransportPlanEditOKButton");
            transportPlanDetail = (TransportPlanDetail)FindNode("TransportPlanDetail");
            transportPlanDetail.ShowName = false;

            remoteMiningWaypointTaskContainer = GetNode<RemoteMiningWaypointTaskContainer>("VBoxContainer/Controls/RemoteMiningWaypointTaskContainer");
            patrolWaypointTaskContainer = GetNode<PatrolWaypointTaskContainer>("VBoxContainer/Controls/PatrolWaypointTaskContainer");
            transferFleetWaypointTaskContainer = GetNode<TransferFleetWaypointTaskContainer>("VBoxContainer/Controls/TransferFleetWaypointTaskContainer");

            layMineFieldWaypointTaksContainer = GetNode<Control>("VBoxContainer/Controls/LayMineFieldWaypointTaskContainer");
            layMineFieldDurationOptionButton = GetNode<OptionButton>("VBoxContainer/Controls/LayMineFieldWaypointTaskContainer/LayMineFieldDurationOptionButton");
            minesLaidPerYearLabel = GetNode<Label>("VBoxContainer/Controls/LayMineFieldWaypointTaskContainer/MinesLaidPerYearLabel");
            layMineFieldDurationOptionButton.PopulateOptionButton<LayMineFieldDuration>((task) => task switch
            {
                LayMineFieldDuration.Indefinitely => "Indefinitely",
                _ => $"for {(int)task} years"
            });

            waypointTask.PopulateOptionButton<WaypointTask>((task) => EnumUtils.GetLabelForWaypointTask(task));

            waypointTask.Connect("item_selected", this, nameof(OnWaypointTaskItemSelected));
            layMineFieldDurationOptionButton.Connect("item_selected", this, nameof(OnLayMineFieldDurationOptionButtonSelected));
            editButton.Connect("pressed", this, nameof(OnEditButtonPressed));
            transportPlanEditOKButton.Connect("pressed", this, nameof(OnTransportPlanEditOKButtonPressed));
            applyPlanMenuButton.GetPopup().Connect("index_pressed", this, nameof(OnApplyPlanMenuButtonIndexPressed));

        }

        protected override void OnNewCommandedFleet()
        {
            base.OnNewCommandedFleet();
            // when we have a new active fleet, set the active waypoint to the
            // first waypoint
            ActiveWaypoint = CommandedFleet?.Fleet.Waypoints[0];
        }


        void OnWaypointTaskItemSelected(int index)
        {
            if (ActiveWaypoint != null && index >= 0 && index < Enum.GetValues(typeof(WaypointTask)).Length)
            {
                log.Debug($"Changing waypoint {ActiveWaypoint.TargetName} from {ActiveWaypoint.Task} to {(WaypointTask)index}");
                ActiveWaypoint.Task = (WaypointTask)index;
            }
            UpdateControls();
        }

        void OnLayMineFieldDurationOptionButtonSelected(int index)
        {
            if (ActiveWaypoint != null && index >= 0 &&
                index < Enum.GetValues(typeof(LayMineFieldDuration)).Length)
            {
                var duration = (LayMineFieldDuration)index;
                ActiveWaypoint.LayMineFieldDuration = duration switch
                {
                    LayMineFieldDuration.Indefinitely => Waypoint.Indefinite,
                    _ => (int)duration
                };
            }
        }

        /// <summary>
        /// When the edfit
        /// </summary>
        void OnEditButtonPressed()
        {
            transportPlanDetail.Plan.Tasks = ActiveWaypoint.TransportTasks;
            transportPlanDetail.UpdateControls();
            transportPlanEditPopupPanel.RectGlobalPosition = new Vector2(RectGlobalPosition.x, RectGlobalPosition.y + RectSize.y);
            transportPlanEditPopupPanel.ShowModal();
        }

        /// <summary>
        /// When the user presses the OK button on the TransportPlan Edit popup, update the tasks for this waypoint
        /// </summary>
        void OnTransportPlanEditOKButtonPressed()
        {
            // update the ActiveWaypoint's transport tasks
            ActiveWaypoint.TransportTasks = transportPlanDetail.Plan.Tasks;
            transportPlanEditPopupPanel.Hide();
            UpdateControls();
        }

        /// <summary>
        /// When the user applies a plan from the player's transport plans, apply the tasks from it here.
        /// </summary>
        /// <param name="index"></param>
        void OnApplyPlanMenuButtonIndexPressed(int index)
        {
            if (index >= 0 && index < Me.TransportPlans.Count)
            {
                ActiveWaypoint.TransportTasks = Me.TransportPlans[index].Tasks;
                UpdateControls();
            }
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            var wp = ActiveWaypoint;
            if (transportContainer != null && remoteMiningWaypointTaskContainer != null)
            {
                transportContainer.Visible = false;
                remoteMiningWaypointTaskContainer.Visible = false;
                layMineFieldWaypointTaksContainer.Visible = false;
                patrolWaypointTaskContainer.Visible = false;
                transferFleetWaypointTaskContainer.Visible = false;

                if (waypointTask != null && wp != null)
                {
                    waypointTask.Selected = (int)wp.Task;

                    if (wp.Task == WaypointTask.Transport)
                    {
                        transportContainer.Visible = true;
                        UpdateTransportTaskIcon(fuelTask, CargoType.Fuel, wp.TransportTasks.Fuel);
                        UpdateTransportTaskIcon(ironiumTask, CargoType.Ironium, wp.TransportTasks.Ironium);
                        UpdateTransportTaskIcon(boraniumTask, CargoType.Boranium, wp.TransportTasks.Boranium);
                        UpdateTransportTaskIcon(germaniumTask, CargoType.Germanium, wp.TransportTasks.Germanium);
                        UpdateTransportTaskIcon(colonistsTask, CargoType.Colonists, wp.TransportTasks.Colonists);

                        applyPlanMenuButton.GetPopup().Clear();
                        Me.TransportPlans.ForEach(plan =>
                        {
                            applyPlanMenuButton.GetPopup().AddItem(plan.Name);
                        });
                    }
                    else if (wp.Task == WaypointTask.RemoteMining)
                    {
                        remoteMiningWaypointTaskContainer.Visible = true;
                        remoteMiningWaypointTaskContainer.Planet = ActiveWaypoint.Target as Planet;
                        remoteMiningWaypointTaskContainer.Fleet = CommandedFleet.Fleet;
                    }
                    else if (wp.Task == WaypointTask.LayMineField)
                    {
                        layMineFieldWaypointTaksContainer.Visible = true;

                        if (wp.LayMineFieldDuration == Waypoint.Indefinite)
                        {
                            layMineFieldDurationOptionButton.Selected = 0;
                        }
                        else
                        {
                            layMineFieldDurationOptionButton.Selected = Mathf.Clamp(wp.LayMineFieldDuration, 1, layMineFieldDurationOptionButton.Items.Count);
                        }

                        int minesLaid = 0;
                        foreach (var entry in CommandedFleet.Fleet.Spec.MineLayingRateByMineType)
                        {
                            minesLaid += entry.Value;
                        }
                        minesLaidPerYearLabel.Text = $"This fleet can lay {minesLaid} mines per year";
                    }
                    else if (wp.Task == WaypointTask.Patrol)
                    {
                        patrolWaypointTaskContainer.Visible = true;
                        patrolWaypointTaskContainer.Waypoint = wp;
                    }
                    else if (wp.Task == WaypointTask.TransferFleet)
                    {
                        transferFleetWaypointTaskContainer.Visible = true;
                        transferFleetWaypointTaskContainer.Waypoint = wp;
                    }
                }

            }
        }

        void UpdateTransportTaskIcon(TextureRect taskTextureRect, CargoType cargoType, WaypointTransportTask transportTask)
        {
            if (transportTask.action == WaypointTaskTransportAction.None)
            {
                taskTextureRect.Texture = noneTexture;
                taskTextureRect.HintTooltip = "No action";
            }
            else
            {
                taskTextureRect.Modulate = Colors.White;

                taskTextureRect.HintTooltip = $"{EnumUtils.GetLabelForWaypointTaskTransportAction(transportTask.action)} {cargoType}";
                switch (transportTask.action)
                {
                    case WaypointTaskTransportAction.LoadAll:
                    case WaypointTaskTransportAction.LoadAmount:
                    case WaypointTaskTransportAction.FillPercent:
                    case WaypointTaskTransportAction.WaitForPercent:
                    case WaypointTaskTransportAction.LoadDunnage:
                        taskTextureRect.Texture = loadTexture;
                        break;
                    case WaypointTaskTransportAction.UnloadAll:
                    case WaypointTaskTransportAction.UnloadAmount:
                        taskTextureRect.Texture = unloadTexture;
                        break;
                    case WaypointTaskTransportAction.SetAmountTo:
                    case WaypointTaskTransportAction.SetWaypointTo:
                    default:
                        taskTextureRect.Texture = noneTexture;
                        taskTextureRect.Modulate = Colors.DimGray;
                        break;
                }
            }
        }
    }
}