using CraigStars.Singletons;
using Godot;
using System;
using System.Collections.Generic;

namespace CraigStars
{

    public class FleetSprite : Node2D
    {
        Sprite selected;
        Sprite active;
        Sprite selectedIndicator;
        Sprite activeIndicator;

        List<Sprite> stateSprites = new List<Sprite>();

        public override void _Ready()
        {
            selected = GetNode<Sprite>("Selected");
            active = GetNode<Sprite>("Active");
            selectedIndicator = GetNode<Sprite>("SelectedIndicator");
            activeIndicator = GetNode<Sprite>("ActiveIndicator");

            stateSprites.Add(selected);
            stateSprites.Add(active);
            stateSprites.Add(selectedIndicator);
            stateSprites.Add(activeIndicator);

        }

        public void UpdateVisibleSprites(Player player, Fleet fleet)
        {
            // turn them all off
            stateSprites.ForEach(s => s.Visible = false);

            // if we are orbiting a planet, don't show any sprites
            if (fleet.Orbiting != null)
            {
                return;
            }

            var ownerAllyState = MapObject.OwnerAlly.Unknown;
            var state = fleet.State;

            if (player != null)
            {
                if (player == PlayersManager.Instance.Me)
                {
                    ownerAllyState = MapObject.OwnerAlly.Owned;
                }
                else
                {
                    ownerAllyState = MapObject.OwnerAlly.Enemy;
                }
            }

            if (state == MapObject.States.Selected)
            {
                selectedIndicator.Visible = true;
                selected.Visible = true;
            }
            else if (state == MapObject.States.Active)
            {
                activeIndicator.Visible = true;
                active.Visible = true;
            }

            switch (ownerAllyState)
            {
                case MapObject.OwnerAlly.Owned:
                    Modulate = Colors.Blue;
                    break;
                case MapObject.OwnerAlly.Friend:
                    Modulate = Colors.Yellow;
                    break;
                case MapObject.OwnerAlly.Enemy:
                    Modulate = Colors.Red;
                    break;
                default:
                    Modulate = Colors.Gray;
                    break;
            }

        }

    }
}