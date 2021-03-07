using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    public class TurnGenerationDialog : WindowDialog
    {
        Player Me { get => PlayersManager.Me; }
        Button cancelButton;
        Label label;
        ProgressBar progressBar;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            cancelButton = (Button)FindNode("CancelButton");
            label = (Label)FindNode("Label");
            progressBar = (ProgressBar)FindNode("ProgressBar");

            Connect("about_to_show", this, nameof(OnAboutToShow));
            Connect("popup_hide", this, nameof(OnPopupHide));
            cancelButton.Connect("pressed", this, nameof(OnCancel));

            Signals.TurnGeneratingEvent += OnTurnGenerating;
            Signals.TurnGeneratorAdvancedEvent += OnTurnGeneratorAdvanced;
            Signals.TurnPassedEvent += OnTurnPassed;
        }

        public override void _ExitTree()
        {
            Signals.TurnGeneratorAdvancedEvent -= OnTurnGeneratorAdvanced;
        }

        void OnTurnGenerating()
        {
            PopupCentered();
            label.Text = "Submitted turn";
            progressBar.Value = 0;
        }

        void OnTurnGeneratorAdvanced(TurnGeneratorState state)
        {
            string labelText;
            switch (state)
            {
                case TurnGeneratorState.WaitingForPlayers:
                    labelText = "Waiting for Players";
                    break;
                default:
                    labelText = state.ToString();
                    break;
            }

            label.Text = labelText;
            progressBar.Value = 100 * ((int)state / (float)(Enum.GetValues(typeof(TurnGeneratorState)).Length));
        }

        void OnTurnPassed(int year)
        {
            Hide();
        }



        void OnAboutToShow()
        {

        }

        void OnPopupHide()
        {
            // nothing to do here, we don't want to save
        }

        void OnCancel()
        {
            // TODO: unsubmit turn
        }

    }

}
