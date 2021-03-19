using CraigStars.Singletons;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    public class MessagesPane : MarginContainer
    {
        public Player Me { get => PlayersManager.Me; }

        PublicGameInfo gameInfo;
        TextEdit messageText;
        Label titleLabel;
        CheckBox filterMessageTypeCheckbox;
        CheckBox filterMessagesCheckbox;

        Button prevButton;
        Button gotoButton;
        Button nextButton;

        int messageNum = 0;

        Message activeMessage;
        List<Message> filteredMessages;

        public override void _Ready()
        {
            messageText = (TextEdit)FindNode("MessageText");
            titleLabel = (Label)FindNode("TitleLabel");
            filterMessageTypeCheckbox = (CheckBox)FindNode("FilterMessageTypeCheckbox");
            filterMessagesCheckbox = (CheckBox)FindNode("FilterMessagesCheckbox");
            prevButton = (Button)FindNode("PrevButton");
            gotoButton = (Button)FindNode("GotoButton");
            nextButton = (Button)FindNode("NextButton");

            nextButton.Connect("pressed", this, nameof(OnNextButtonPressed));
            gotoButton.Connect("pressed", this, nameof(OnGotoButtonPressed));
            prevButton.Connect("pressed", this, nameof(OnPrevButtonPressed));
            filterMessageTypeCheckbox.Connect("toggled", this, nameof(OnFilterMessageTypeCheckboxToggled));
            filterMessagesCheckbox.Connect("toggled", this, nameof(OnFilterMessagesCheckboxToggled));

            Signals.TurnPassedEvent += OnTurnPassed;
            Signals.PostStartGameEvent += OnPostStartGame;
            Signals.PlayerDirtyEvent += OnPlayerDirty;
            UpdateControls();
        }

        public override void _ExitTree()
        {
            Signals.TurnPassedEvent -= OnTurnPassed;
            Signals.PostStartGameEvent -= OnPostStartGame;
            Signals.PlayerDirtyEvent -= OnPlayerDirty;
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

        void OnFilterMessageTypeCheckboxToggled(bool buttonPressed)
        {
            if (activeMessage != null)
            {
                if (buttonPressed)
                {
                    Me.UISettings.MessageTypeFilter |= (ulong)activeMessage.Type;
                }
                else
                {
                    // turn off this
                    Me.UISettings.MessageTypeFilter &= ~(ulong)activeMessage.Type;
                }

                Me.Dirty = true;
                Signals.PublishPlayerDirtyEvent();

                UpdateControls();
            }
        }

        void OnFilterMessagesCheckboxToggled(bool buttonPressed)
        {
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
            messageNum = 0;
            if (Me != null && Me.Messages.Count > 0)
            {
                activeMessage = Me.Messages[0];
            }
            UpdateControls();
        }

        void OnPlayerDirty()
        {
            UpdateControls();
        }

        void UpdateControls()
        {
            var changesMadeIndicator = Me.Dirty ? "*" : "";

            var messages = filterMessagesCheckbox.Pressed ? Me.FilteredMessages.ToList() : Me.Messages;

            // show the FilterMessagesCheckbox if we are filtering messages
            // if we are showing all messages, don't show it.
            filterMessagesCheckbox.Visible = Me.UISettings.MessageTypeFilter != ulong.MaxValue;

            if (messages.Count > 0)
            {
                filterMessageTypeCheckbox.Visible = true;
                titleLabel.Text = $"Year: {gameInfo.Year}{changesMadeIndicator} Message {messageNum + 1} of {messages.Count}{(filterMessagesCheckbox.Pressed ? $" ({Me.Messages.Count} total)" : "")}";
                if (messageNum >= 0 && messageNum < Me.Messages.Count)
                {
                    activeMessage = messages[messageNum];
                    messageText.Text = activeMessage.Text;
                }

                filterMessageTypeCheckbox.Pressed = (Me.UISettings.MessageTypeFilter & (ulong)activeMessage.Type) > 0;
                // disable/enable buttons
                prevButton.Disabled = messageNum == 0;
                nextButton.Disabled = messageNum >= messages.Count - 1;
                gotoButton.Disabled = activeMessage?.Target == null;

            }
            else
            {
                filterMessageTypeCheckbox.Visible = false;
                titleLabel.Text = $"Year: {gameInfo?.Year}{changesMadeIndicator} No Messages{(filterMessagesCheckbox.Pressed ? $" ({Me.Messages.Count} filtered)" : "")}";
                prevButton.Disabled = nextButton.Disabled = gotoButton.Disabled = true;
                activeMessage = null;
                messageNum = 0;
            }

        }
    }
}
