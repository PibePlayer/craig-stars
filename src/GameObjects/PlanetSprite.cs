using System.Collections.Generic;
using CraigStars.Singletons;
using Godot;

namespace CraigStars
{
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

        public void UpdateVisibleSprites(Player player, Planet planet)
        {
            var ownerAllyState = MapObject.OwnerAlly.Unknown;
            var state = planet.State;
            var hasActivePeer = planet.HasActivePeer();

            // TODO: make this work with multiple types
            if (planet.OrbitingFleets.Count > 0)
            {
                planet.OrbitingState = Planet.Orbiting.Orbiting;
            }
            else
            {
                planet.OrbitingState = Planet.Orbiting.None;
            }

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

            if (state == MapObject.States.Active || hasActivePeer)
            {
                activeIndicator.Visible = true;
            }
            else if (state == MapObject.States.Selected)
            {
                selectedIndicator.Visible = true;
            }

            switch (ownerAllyState)
            {
                case MapObject.OwnerAlly.Unknown:
                    unknown.Visible = true;
                    break;
                case MapObject.OwnerAlly.Known:
                    if (hasActivePeer || state == MapObject.States.Active)
                    {
                        knownActive.Visible = true;
                    }
                    else
                    {
                        known.Visible = true;
                    }
                    break;
                case MapObject.OwnerAlly.Owned:
                    if (hasActivePeer || state == MapObject.States.Active)
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
            switch (planet.OrbitingState)
            {
                case Planet.Orbiting.Orbiting:
                case Planet.Orbiting.OrbitingEnemies:
                case Planet.Orbiting.OrbitingAlliesAndEnemies:
                    if (hasActivePeer || state == MapObject.States.Active)
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
}
