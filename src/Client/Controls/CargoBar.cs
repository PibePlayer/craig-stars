using System;
using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client
{
    public class CargoBar : Control
    {
        static CSLog log = LogProvider.GetLogger(typeof(CargoBar));

        /// <summary>
        /// delegate function called when a value on the bar is clicked, or dragged, reporting the updated value
        /// </summary>
        /// <param name="newValue"></param>
        public delegate void ValueUpdated(int newValue);

        /// <summary>
        /// Event fired when the value of the bar is updated by user input
        /// </summary>
        public event ValueUpdated ValueUpdatedEvent;

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

        public int Fuel
        {
            get => fuel;
            set
            {
                fuel = value;
                Update();
            }
        }
        int fuel = 0;

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

            SetProcess(false);
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            if (updatingValue == false)
            {
                SetProcess(false);
            }
            else
            {
                Vector2 mousePosition = GetLocalMousePosition();
                int valueFromClick = (int)Mathf.Clamp((int)Math.Round(mousePosition.x / (panel.RectSize.x - borderWidth) * Capacity), 0, Capacity);
                ValueUpdatedEvent?.Invoke(valueFromClick);

            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionReleased("viewport_select"))
            {
                updatingValue = false;
            }
        }

        void OnGUIInput(InputEvent @event)
        {
            if (@event.IsActionPressed("viewport_select"))
            {
                updatingValue = true;
                SetProcess(true);
            }
            else if (@event.IsActionReleased("viewport_select"))
            {
                updatingValue = false;
            }
        }


        public override void _Draw()
        {
            if (panel != null)
            {
                if (IsFuel)
                {

                    label.Text = $"{Fuel} of {Capacity}{Unit}";

                    if (Fuel > 0)
                    {
                        // get the width of our rectangle
                        // it's a percentage of the totaly width based on capacity and quantity
                        float width = panel.RectSize.x * ((float)Fuel / (float)Capacity);
                        DrawRect(new Rect2(
                            new Vector2(panel.RectPosition.x + borderWidth / 2, panel.RectPosition.y + borderHeight / 2),
                            new Vector2(width - borderWidth, panel.RectSize.y - (borderHeight))),
                            GUIColorsProvider.Colors.FuelColor
                        );
                    }
                }
                else
                {
                    label.Text = $"{Cargo.Total} of {Capacity}{Unit}";
                    float ironiumWidth = 0;
                    float boraniumWidth = 0;
                    float germaniumWidth = 0;
                    float colonistsWidth = 0;

                    if (Cargo.Ironium > 0)
                    {
                        ironiumWidth = panel.RectSize.x * ((float)Cargo.Ironium / (float)Capacity);
                        DrawRect(new Rect2(
                            new Vector2(panel.RectPosition.x + borderWidth / 2, panel.RectPosition.y + borderHeight / 2),
                            new Vector2(ironiumWidth - borderWidth, panel.RectSize.y - borderHeight)),
                            GUIColorsProvider.Colors.IroniumBarColor
                        );
                    }
                    if (Cargo.Boranium > 0)
                    {
                        boraniumWidth = panel.RectSize.x * ((float)Cargo.Boranium / (float)Capacity);
                        DrawRect(new Rect2(
                            new Vector2(panel.RectPosition.x + borderWidth / 2 + ironiumWidth, panel.RectPosition.y + borderHeight / 2),
                            new Vector2(boraniumWidth - borderWidth, panel.RectSize.y - borderHeight)),
                            GUIColorsProvider.Colors.BoraniumBarColor
                        );
                    }
                    if (Cargo.Germanium > 0)
                    {
                        germaniumWidth = panel.RectSize.x * ((float)Cargo.Germanium / (float)Capacity);
                        DrawRect(new Rect2(
                            new Vector2(panel.RectPosition.x + borderWidth / 2 + ironiumWidth + boraniumWidth, panel.RectPosition.y + borderHeight / 2),
                            new Vector2(germaniumWidth - borderWidth, panel.RectSize.y - borderHeight)),
                            GUIColorsProvider.Colors.GermaniumBarColor
                        );
                    }
                    if (Cargo.Colonists > 0)
                    {
                        colonistsWidth = panel.RectSize.x * ((float)Cargo.Colonists / (float)Capacity);
                        DrawRect(new Rect2(
                            new Vector2(panel.RectPosition.x + borderWidth / 2 + ironiumWidth + boraniumWidth + germaniumWidth, panel.RectPosition.y + borderHeight / 2),
                            new Vector2(colonistsWidth - borderWidth, panel.RectSize.y - borderHeight)),
                            Colors.White
                        );

                    }

                }
            }
        }

    }
}