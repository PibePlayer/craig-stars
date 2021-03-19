using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    public class MessagesPane : MarginContainer
    {
        PublicGameInfo gameInfo;
        TextEdit messageText;
        Label titleLabel;
        CheckBox filterMessageTypeCheckbox;
        CheckBox showFilteredMessagesCheckbox;

        Button prevButton;
        Button gotoButton;
        Button nextButton;

        int messageNum = 0;
        bool changesMade = false;

        Message activeMessage;

        public override void _Ready()
        {
            messageText = FindNode("MessageText") as TextEdit;
            titleLabel = FindNode("TitleLabel") as Label;
            filterMessageTypeCheckbox = FindNode("FilterMessageTypeCheckbox") as CheckBox;
            showFilteredMessagesCheckbox = FindNode("ShowFilteredMessagesCheckbox") as CheckBox;
            prevButton = FindNode("PrevButton") as Button;
            gotoButton = FindNode("GotoButton") as Button;
            nextButton = FindNode("NextButton") as Button;

            nextButton.Connect("pressed", this, nameof(OnNextButtonPressed));
            gotoButton.Connect("pressed", this, nameof(OnGotoButtonPressed));
            prevButton.Connect("pressed", this, nameof(OnPrevButtonPressed));

            Signals.TurnPassedEvent += OnTurnPassed;
            Signals.PostStartGameEvent += OnPostStartGame;
            UpdateControls();
        }

        public override void _ExitTree()
        {
            Signals.TurnPassedEvent -= OnTurnPassed;
            Signals.PostStartGameEvent -= OnPostStartGame;
        }

        void OnNextButtonPressed()
        {
            messageNum++;
            UpdateControls();
        }

        void OnGotoButtonPressed()
        {
            if (activeMessage != null && activeMessage.Target != null)
            {
                Signals.PublishGotoMapObjectEvent(activeMessage.Target);
            }
        }

        void OnPrevButtonPressed()
        {
            messageNum--;
            UpdateControls();
        }

        void OnPostStartGame(PublicGameInfo gameInfo)
        {
            this.gameInfo = gameInfo;
            OnTurnPassed(gameInfo);
        }

        void OnTurnPassed(PublicGameInfo gameInfo)
        {
            this.gameInfo = gameInfo;
            var player = PlayersManager.Me;
            messageNum = 0;
            if (player != null && player.Messages.Count > 0)
            {
                activeMessage = player.Messages[0];
            }
            UpdateControls();
        }

        void UpdateControls()
        {
            var player = PlayersManager.Me;
            var changesMadeIndicator = changesMade ? "*" : "";
            if (player.Messages.Count > 0)
            {
                titleLabel.Text = $"Year: {gameInfo.Year}{changesMadeIndicator} Message {messageNum + 1} of {player.Messages.Count}";
                if (messageNum >= 0 && messageNum < player.Messages.Count)
                {
                    activeMessage = player.Messages[messageNum];
                    messageText.Text = activeMessage.Text;
                }

                // disable/enable buttons
                prevButton.Disabled = messageNum == 0;
                nextButton.Disabled = messageNum >= player.Messages.Count - 1;
                gotoButton.Disabled = activeMessage?.Target == null;

            }
            else
            {
                titleLabel.Text = $"Year: {gameInfo?.Year}{changesMadeIndicator} No Messages";
                prevButton.Disabled = nextButton.Disabled = gotoButton.Disabled = true;
                activeMessage = null;
                messageNum = 0;
            }

        }
    }
}
