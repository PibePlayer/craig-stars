using CraigStars.Singletons;
using Godot;
using System;
using System.Collections.Generic;

namespace CraigStars
{

    public class FleetSprite : MapObjectSprite<Fleet>
    {
        Sprite selected;
        Sprite active;

        List<Sprite> stateSprites = new List<Sprite>();

        public override void _Ready()
        {
            selected = GetNode<Sprite>("Selected");
            active = GetNode<Sprite>("Active");

            stateSprites.Add(selected);
            stateSprites.Add(active);

        }

        public override void UpdateSprite(Player player, Fleet fleet)
        {
            if (player.Num == 0)
            {
                GD.Print($"Updating Fleet sprite {fleet.ObjectName} for player {player.Num}");
            }
            // turn them all off

            stateSprites.ForEach(s => s.Visible = false);

            // if we are orbiting a planet, don't show any sprites
            if (fleet.Orbiting != null)
            {
                return;
            }

            Sprite shipSprite = fleet.State == MapObject.States.Active ? active : selected;
            shipSprite.Visible = true;

            var ownerAllyState = MapObject.OwnerAlly.Unknown;
            if (fleet.OwnedByMe)
            {
                ownerAllyState = MapObject.OwnerAlly.Owned;
            }
            else
            {
                ownerAllyState = MapObject.OwnerAlly.Enemy;
            }

            switch (ownerAllyState)
            {
                case MapObject.OwnerAlly.Owned:
                    shipSprite.Modulate = Colors.Blue;
                    break;
                case MapObject.OwnerAlly.Friend:
                    shipSprite.Modulate = Colors.Yellow;
                    break;
                case MapObject.OwnerAlly.Enemy:
                    shipSprite.Modulate = Colors.Red;
                    break;
                default:
                    shipSprite.Modulate = Colors.Gray;
                    break;
            }

        }

    }
}