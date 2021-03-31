using Godot;
using log4net;
using System;
using System.Collections.Generic;
using CraigStars.Singletons;

namespace CraigStars
{
    [Tool]
    public class BattleGridSquare : MarginContainer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BattleGridSquare));

        public event Action<BattleGridSquare> SelectedEvent;

        public Vector2 Coordinates { get; set; }

        [Export]
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                if (selected && panelStyleBox != null)
                {
                    panelStyleBox.BorderColor = Colors.Blue;
                }
                else if (!selected && panelStyleBox != null)
                {
                    panelStyleBox.BorderColor = originalBorderColor;
                }
            }
        }
        bool selected = false;

        public int SelectedTokenIndex
        {
            get => selectedTokenIndex;
            set
            {
                selectedTokenIndex = value;
                if (selectedTokenIndex < Tokens.Count)
                {
                    icon.Texture = TextureLoader.Instance.FindTexture(Tokens[selectedTokenIndex].Token.Design);
                }
            }
        }
        int selectedTokenIndex;

        public List<BattleRecordToken> Tokens { get; set; } = new List<BattleRecordToken>();
        public int NumShips { get; private set; } = 0;
        public int NumTokens { get; private set; } = 0;

        TextureRect icon;
        Panel iconPanel;
        StyleBoxFlat panelStyleBox;
        Color originalBorderColor = Colors.White;

        Label numTokensLabel;
        Label numShipsLabel;

        public override void _Ready()
        {
            numTokensLabel = GetNode<Label>("NumTokensLabel");
            numShipsLabel = GetNode<Label>("NumShipsLabel");
            icon = GetNode<TextureRect>("Icon");
            iconPanel = GetNode<Panel>("Icon/Panel");
            panelStyleBox = iconPanel.GetStylebox("panel") as StyleBoxFlat;
            originalBorderColor = panelStyleBox.BorderColor;

            icon.Connect("gui_input", this, nameof(OnGUIInput));

            Connect("visibility_changed", this, nameof(OnVisibilityChanged));
        }

        public void ClearTokens()
        {
            Tokens.Clear();
            icon.Texture = null;
            NumShips = NumTokens = 0;
            numTokensLabel.Text = "";
            numShipsLabel.Text = "";
        }

        public void AddToken(BattleRecordToken token)
        {
            Tokens.Add(token);
            NumTokens++;
            NumShips += token.Token.Quantity;

            numTokensLabel.Text = $"{NumTokens}";
            numShipsLabel.Text = $"{NumShips}";
            if (icon.Texture == null)
            {
                icon.Texture = TextureLoader.Instance.FindTexture(token.Token.Design);
            }
        }

        public void RemoveToken(BattleRecordToken token)
        {
            Tokens.Remove(token);
            NumTokens--;
            NumShips -= token.Token.Quantity;


            if (Tokens.Count > 0)
            {
                numTokensLabel.Text = $"{NumTokens}";
                numShipsLabel.Text = $"{NumShips}";
                icon.Texture = TextureLoader.Instance.FindTexture(Tokens[0].Token.Design);
            }
            else
            {
                numTokensLabel.Text = "";
                numShipsLabel.Text = "";
                icon.Texture = null;
            }
        }

        void OnVisibilityChanged()
        {
            if (Visible)
            {
                if (Tokens.Count > 0)
                {
                }
            }
        }

        void OnGUIInput(InputEvent @event)
        {
            if (@event.IsActionPressed("viewport_select"))
            {
                SelectedEvent?.Invoke(this);
                log.Debug("BattleGridSquare clicked");
                // Selected = !Selected;
                GetTree().SetInputAsHandled();
            }
        }
    }
}
