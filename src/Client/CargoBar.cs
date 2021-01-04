using Godot;
using System;

namespace CraigStars
{
    public class CargoBar : Control
    {
        [Export]
        public GUIColors GUIColors { get; set; } = new GUIColors();

        [Export]
        public bool IsFuel
        {
            get => isFuel;
            set
            {
                isFuel = value;
                Update();
            }
        }
        bool isFuel;

        public Cargo Cargo
        {
            get => cargo;
            set
            {
                cargo = value;
                Update();
            }
        }
        Cargo cargo = new Cargo(3, 3, 2, 8);

        [Export]
        public int Capacity
        {
            get => capacity;
            set
            {
                capacity = value;
                Update();
            }
        }
        int capacity = 25;

        [Export]
        public string Unit
        {
            get => unit;
            set
            {
                unit = value;
                Update();
            }
        }
        string unit = "kT";

        Panel panel;
        Label label;
        StyleBoxFlat panelStyleBox;
        int borderWidth;
        int borderHeight;
        bool updatingValue = false;

        public override void _Ready()
        {
            panel = GetNode<Panel>("Panel");
            label = GetNode<Label>("Label");
            panelStyleBox = panel.GetStylebox("panel") as StyleBoxFlat;

            panel.Connect("gui_input", this, nameof(OnGUIInput));

            borderWidth = panelStyleBox.BorderWidthLeft + panelStyleBox.BorderWidthRight;
            borderHeight = panelStyleBox.BorderWidthTop + panelStyleBox.BorderWidthBottom;
            Update();
        }


        void OnGUIInput(InputEvent @event)
        {
            if (@event.IsActionPressed("viewport_select"))
            {
                updatingValue = true;
            }
            else if (@event.IsActionReleased("viewport_select"))
            {
                updatingValue = false;
            }
            if (updatingValue && @event is InputEventMouse mouse)
            {
                Vector2 mousePosition = mouse.Position;
                mousePosition = mouse.Position;
                int valueFromClick = (int)(Math.Round(mousePosition.x / (panel.RectSize.x - borderWidth) * Capacity));
                GD.Print($"Mouse clicked {mousePosition} for warp speed {valueFromClick}");
                if (valueFromClick >= 0 && valueFromClick <= Capacity)
                {
                    // TODO: do something with this update to signal we are transferring
                }
            }
        }

        public override void _Draw()
        {
            if (panel != null)
            {
                if (IsFuel)
                {

                    label.Text = $"{Cargo.Fuel} of {Capacity}{Unit}";

                    if (Cargo.Fuel > 0)
                    {
                        // get the width of our rectangle
                        // it's a percentage of the totaly width based on capacity and quantity
                        float width = panel.RectSize.x * ((float)Cargo.Fuel / (float)Capacity) - (borderWidth);
                        DrawRect(new Rect2(
                            panel.RectPosition,
                            new Vector2(width, panel.RectSize.y - (borderHeight / 2))),
                            GUIColors.FuelColor
                        );
                    }
                }
                else
                {
                    label.Text = $"{Cargo.Total} of {Capacity}{Unit}";
                    float ironiumWidth = panel.RectSize.x * ((float)Cargo.Ironium / (float)Capacity) - (borderWidth);
                    float boraniumWidth = panel.RectSize.x * ((float)Cargo.Boranium / (float)Capacity) - (borderWidth);
                    float germaniumWidth = panel.RectSize.x * ((float)Cargo.Germanium / (float)Capacity) - (borderWidth);
                    float colonistsWidth = panel.RectSize.x * ((float)Cargo.Colonists / (float)Capacity) - (borderWidth);

                    if (Cargo.Ironium > 0)
                    {
                        DrawRect(new Rect2(
                            panel.RectPosition,
                            new Vector2(ironiumWidth, panel.RectSize.y - (borderHeight / 2))),
                            GUIColors.IroniumBarColor
                        );
                    }
                    if (Cargo.Boranium > 0)
                    {
                        DrawRect(new Rect2(
                            new Vector2(panel.RectPosition.x + ironiumWidth, panel.RectPosition.y),
                            new Vector2(boraniumWidth, panel.RectSize.y - (borderHeight / 2))),
                            GUIColors.BoraniumBarColor
                        );
                    }
                    if (Cargo.Germanium > 0)
                    {
                        DrawRect(new Rect2(
                            new Vector2(panel.RectPosition.x + ironiumWidth + boraniumWidth, panel.RectPosition.y),
                            new Vector2(germaniumWidth, panel.RectSize.y - (borderHeight / 2))),
                            GUIColors.GermaniumBarColor
                        );
                    }
                    if (Cargo.Colonists > 0)
                    {
                        DrawRect(new Rect2(
                            new Vector2(panel.RectPosition.x + ironiumWidth + boraniumWidth + germaniumWidth, panel.RectPosition.y),
                            new Vector2(colonistsWidth, panel.RectSize.y - (borderHeight / 2))),
                            Colors.White
                        );

                    }

                }
            }
        }

    }
}