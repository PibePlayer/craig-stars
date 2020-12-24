using Godot;
using System;
using System.Collections.Generic;

public class PlanetSprite : Node2D
{
    Sprite known;
    Sprite unknown;
    Sprite owned;
    Sprite friend;
    Sprite enemy;
    Sprite knownActive;
    Sprite ownedActive;
    Sprite orbiting;
    Sprite orbitingActive;
    Sprite selectedIndicator;
    Sprite activeIndicator;

    List<Sprite> stateSprites = new List<Sprite>();

    public override void _Ready()
    {
        known = GetNode<Sprite>("Known");
        unknown = GetNode<Sprite>("Unknown");
        owned = GetNode<Sprite>("Owned");
        friend = GetNode<Sprite>("Friend");
        enemy = GetNode<Sprite>("Enemy");
        knownActive = GetNode<Sprite>("KnownActive");
        ownedActive = GetNode<Sprite>("OwnedActive");
        orbiting = GetNode<Sprite>("Orbiting");
        orbitingActive = GetNode<Sprite>("OrbitingActive");
        selectedIndicator = GetNode<Sprite>("SelectedIndicator");
        activeIndicator = GetNode<Sprite>("ActiveIndicator");

        // create a list of these sprites
        stateSprites.Add(known);
        stateSprites.Add(unknown);
        stateSprites.Add(owned);
        stateSprites.Add(friend);
        stateSprites.Add(enemy);
        stateSprites.Add(knownActive);
        stateSprites.Add(ownedActive);
        stateSprites.Add(orbiting);
        stateSprites.Add(orbitingActive);
        stateSprites.Add(selectedIndicator);
        stateSprites.Add(activeIndicator);
    }

    public void UpdateVisibleSprites(Player player, MapObject.OwnerAlly ownerAllyState, Planet.States state, Planet.Orbiting orbitingState)
    {
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
        // turn them all off
        stateSprites.ForEach(s => s.Visible = false);

        if (state == MapObject.States.Selected)
        {
            selectedIndicator.Visible = true;
        }
        else if (state == MapObject.States.Active)
        {
            activeIndicator.Visible = true;
        }

        switch (ownerAllyState)
        {
            case MapObject.OwnerAlly.Unknown:
                unknown.Visible = true;
                break;
            case MapObject.OwnerAlly.Known:
                if (state == MapObject.States.Active)
                {
                    knownActive.Visible = true;
                }
                else
                {
                    known.Visible = true;
                }
                break;
            case MapObject.OwnerAlly.Owned:
                if (state == MapObject.States.Active)
                {
                    ownedActive.Visible = true;
                }
                else
                {
                    owned.Visible = true;
                }
                break;
            case MapObject.OwnerAlly.Friend:
                friend.Visible = true;
                break;
            case MapObject.OwnerAlly.Enemy:
                enemy.Visible = true;
                break;
        }

        // turn on the orbiting ring
        switch (orbitingState)
        {
            case Planet.Orbiting.Orbiting:
            case Planet.Orbiting.OrbitingEnemies:
            case Planet.Orbiting.OrbitingAlliesAndEnemies:
                if (state == MapObject.States.Active)
                {
                    orbitingActive.Visible = true;
                }
                else
                {
                    orbiting.Visible = true;
                }
                break;
        }

    }
}
