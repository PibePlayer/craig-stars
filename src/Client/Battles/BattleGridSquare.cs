using Godot;
using log4net;
using System;
using System.Collections.Generic;
using CraigStars.Singletons;

namespace CraigStars
{
    [Tool]
    public class BattleGridSquare : Panel
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BattleGridSquare));

        public event Action<BattleGridSquare, BattleGridToken> SelectedEvent;

        public Vector2 Coordinates { get; set; }

        [Export]
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                if (selected && styleBox != null)
                {
                    styleBox.BorderColor = Colors.Blue;
                }
                else if (!selected && styleBox != null)
                {
                    styleBox.BorderColor = originalBorderColor;
                }
            }
        }
        bool selected = false;

        public int SelectedTokenIndex
        {
            get => selectedTokenIndex;
            set
            {
                // turn off the previously selected token
                if (SelectedToken != null)
                {
                    SelectedToken.Visible = false;
                }

                // turn on the currently selected token
                selectedTokenIndex = value;
                if (selectedTokenIndex != -1 && selectedTokenIndex < Tokens.Count)
                {
                    SelectedToken = Tokens[selectedTokenIndex];
                    SelectedToken.Visible = true;
                } else {
                    SelectedToken = null;
                }
            }
        }
        int selectedTokenIndex = -1;

        public BattleGridToken SelectedToken { get; set; }
        public List<BattleGridToken> Tokens { get; set; } = new List<BattleGridToken>();
        public int NumShips { get; private set; } = 0;
        public int NumTokens { get; private set; } = 0;

        StyleBoxFlat styleBox;
        Color originalBorderColor = Colors.White;

        Control tokenContainer;
        Label numTokensLabel;
        Label numShipsLabel;

        public override void _Ready()
        {
            tokenContainer = GetNode<Control>("TokenContainer");
            numTokensLabel = GetNode<Label>("NumTokensLabel");
            numShipsLabel = GetNode<Label>("NumShipsLabel");
            styleBox = GetStylebox("panel") as StyleBoxFlat;
            originalBorderColor = styleBox.BorderColor;

            Connect("gui_input", this, nameof(OnGUIInput));
        }

        public void ClearTokens()
        {
            // remove tokens from this node
            foreach (Node child in tokenContainer.GetChildren())
            {
                tokenContainer.RemoveChild(child);
            }
            SelectedToken = null;
            SelectedTokenIndex = -1;
            Tokens.Clear();
            NumShips = NumTokens = 0;
            numTokensLabel.Text = "";
            numShipsLabel.Text = "";
        }

        public void AddToken(BattleGridToken token)
        {
            tokenContainer.AddChild(token);
            Tokens.Add(token);
            SelectedTokenIndex = Tokens.Count - 1;
            NumTokens++;
            NumShips += token.Quantity;

            numTokensLabel.Text = $"{NumTokens}";
            numShipsLabel.Text = $"{NumShips}";
        }

        public void RemoveToken(BattleGridToken token)
        {
            Tokens.Remove(token);
            tokenContainer.RemoveChild(token);
            NumTokens--;
            NumShips -= token.Quantity;

            if (SelectedToken == token)
            {
                if (Tokens.Count > 0)
                {
                    // we removed our visible token, so select the top one
                    SelectedTokenIndex = 0;
                }
                else
                {
                    SelectedTokenIndex = -1;
                }
            }

            if (Tokens.Count > 0)
            {
                numTokensLabel.Text = $"{NumTokens}";
                numShipsLabel.Text = $"{NumShips}";
            }
            else
            {
                numTokensLabel.Text = "";
                numShipsLabel.Text = "";
            }
        }

        void OnGUIInput(InputEvent @event)
        {
            if (@event.IsActionPressed("viewport_select"))
            {
                SelectedEvent?.Invoke(this, SelectedToken);
                log.Debug("BattleGridSquare clicked");
                // Selected = !Selected;
                GetTree().SetInputAsHandled();
            }
        }
    }
}
