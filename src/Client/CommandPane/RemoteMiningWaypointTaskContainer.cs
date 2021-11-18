using CraigStars.Client;
using CraigStars.Utils;
using Godot;
using System;

namespace CraigStars.Client
{
    public class RemoteMiningWaypointTaskContainer : VBoxContainer
    {
        [Inject] PlanetService planetService;

        Label remoteMiningLabel;

        Container remoteMiningSummaryContainer;
        Label ironium;
        Label boranium;
        Label germanium;

        public Planet Planet
        {
            get => planet;
            set
            {
                planet = value;
                UpdateControls();
            }
        }
        Planet planet;

        public Fleet Fleet
        {
            get => fleet;
            set
            {
                fleet = value;
                UpdateControls();
            }
        }
        Fleet fleet;

        public override void _Ready()
        {
            this.ResolveDependencies();

            remoteMiningLabel = GetNode<Label>("RemoteMiningLabel");

            remoteMiningSummaryContainer = GetNode<Container>("RemoteMiningSummaryContainer");
            ironium = GetNode<Label>("RemoteMiningSummaryContainer/Ironium");
            boranium = GetNode<Label>("RemoteMiningSummaryContainer/Boranium");
            germanium = GetNode<Label>("RemoteMiningSummaryContainer/Germanium");
        }

        void UpdateControls()
        {
            remoteMiningSummaryContainer.Visible = false;
            remoteMiningLabel.Modulate = Colors.Red;

            // update our mining stats
            if (Fleet != null && Planet != null && fleet.Spec.MiningRate > 0 && Planet.Explored && !Planet.Owned)
            {
                remoteMiningSummaryContainer.Visible = true;

                remoteMiningLabel.Modulate = Colors.White;
                remoteMiningLabel.Text = "Mining Rate per Year:";
                Mineral output = planetService.GetMineralOutput(Planet, Fleet.Spec.MiningRate);
                ironium.Text = output.Ironium.ToString();
                boranium.Text = output.Boranium.ToString();
                germanium.Text = output.Germanium.ToString();
            }
            else
            {
                if (Planet != null && Planet.Owned)
                {
                    remoteMiningLabel.Text = "Note: You can only remote mine unoccupied planets.";
                }
                else if (Planet != null && !Planet.Explored)
                {
                    remoteMiningLabel.Text = "Warning: This planet is unexplored. We have no way of knowing if we can mine it.";
                }
                else if (!(fleet?.Spec?.MiningRate > 0))
                {
                    remoteMiningLabel.Text = "Warning: This fleet contains no ships with remote mining modules.";
                }
                else
                {
                    remoteMiningLabel.Text = "Warning: Something went wrong.";
                }
            }

        }

    }
}