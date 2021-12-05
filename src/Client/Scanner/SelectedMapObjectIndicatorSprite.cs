using System;
using Godot;

namespace CraigStars.Client
{
    public class SelectedMapObjectIndicatorSprite : Node2D
    {
        Sprite selected;
        Sprite selectedLarge;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            selected = GetNode<Sprite>("Selected");
            selectedLarge = GetNode<Sprite>("SelectedLarge");
        }

        public void Select(Vector2 position)
        {
            Position = position;
            selected.Visible = true;
            selectedLarge.Visible = false;
        }

        public void SelectLarge(Vector2 position)
        {
            Position = position;
            selected.Visible = false;
            selectedLarge.Visible = true;
        }
    }
}