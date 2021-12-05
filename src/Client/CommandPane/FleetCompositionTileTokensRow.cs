using System;
using Godot;

namespace CraigStars.Client
{
    public class FleetCompositionTileTokensRow : Control
    {
        public event Action<FleetCompositionTileTokensRow> SelectedEvent;
        public ShipToken Token
        {
            get => token;
            set
            {
                token = value;
                UpdateControls();
            }
        }
        ShipToken token;

        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                if (selected)
                {
                    SelectedEvent?.Invoke(this);
                }
                Update();
            }
        }
        bool selected = false;

        Label name;
        Label quantity;

        public override void _Ready()
        {
            name = GetNode<Label>("HBoxContainer/Name");
            quantity = GetNode<Label>("HBoxContainer/Quantity");

            Connect("gui_input", this, nameof(OnGUIInput));
            UpdateControls();
        }

        public override void _Draw()
        {
            base._Draw();
            if (Selected)
            {
                // draw a seleciton rect
                DrawRect(new Rect2(2, 2, RectSize.x - 4, RectSize.y - 4), new Color(Colors.White, .125f));
            }

            if (Token.Damage > 0)
            {
                var damagePercent = (Token.Damage * Token.QuantityDamaged) / (token.Design.Armor * Token.Quantity);
                // draw a seleciton rect
                DrawRect(new Rect2(2, 2, (RectSize.x - 4) * damagePercent, RectSize.y - 4), Colors.Red);
            }
        }

        void OnGUIInput(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_select"))
            {
                Selected = true;
            }
        }

        protected void UpdateControls()
        {
            if (name != null && Token != null)
            {
                name.Text = token.Design.Name;
                quantity.Text = $"{token.Quantity}";
            }
        }

    }
}