using System.Collections.Generic;
using CraigStars.Singletons;
using Godot;

namespace CraigStars
{
    public class PlanetSprite : MapObjectSprite<Planet>
    {
        GUIColors guiColors = new GUIColors();

        Sprite known;
        Sprite unknown;
        Sprite inhabited;
        Sprite inhabitedCommanded;
        Sprite orbiting;
        Sprite orbitingCommanded;

        List<Sprite> stateSprites = new List<Sprite>();

        public override void _Ready()
        {
            guiColors = GD.Load("res://src/GUI/GUIColors.tres") as GUIColors;
            if (guiColors == null)
            {
                guiColors = new GUIColors();
            }

            known = GetNode<Sprite>("Known");
            unknown = GetNode<Sprite>("Unknown");
            inhabited = GetNode<Sprite>("Inhabited");
            inhabitedCommanded = GetNode<Sprite>("InhabitedCommanded");
            orbiting = GetNode<Sprite>("Orbiting");
            orbitingCommanded = GetNode<Sprite>("OrbitingActive");

            // create a list of these sprites
            stateSprites.Add(known);
            stateSprites.Add(unknown);
            stateSprites.Add(inhabited);
            stateSprites.Add(inhabitedCommanded);
            stateSprites.Add(orbiting);
            stateSprites.Add(orbitingCommanded);
        }

        public override void UpdateSprite(Player player, Planet planet)
        {
            var ownerAllyState = planet.ReportAge == Planet.Unexplored ? MapObject.OwnerAlly.Unknown : MapObject.OwnerAlly.Known;
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

            switch (ownerAllyState)
            {
                case MapObject.OwnerAlly.Unknown:
                    unknown.Visible = true;
                    break;
                case MapObject.OwnerAlly.Known:
                    if (hasActivePeer || state == MapObject.States.Active)
                    {
                        inhabitedCommanded.Visible = true;
                    }
                    else
                    {
                        known.Visible = true;
                    }
                    break;
                case MapObject.OwnerAlly.Owned:
                    if (hasActivePeer || state == MapObject.States.Active)
                    {
                        inhabitedCommanded.Visible = true;
                        inhabitedCommanded.Modulate = guiColors.OwnedColor;
                    }
                    else
                    {
                        inhabited.Visible = true;
                        inhabited.Modulate = guiColors.OwnedColor;
                    }
                    break;
                case MapObject.OwnerAlly.Friend:
                    if (hasActivePeer || state == MapObject.States.Active)
                    {
                        inhabitedCommanded.Visible = true;
                        inhabitedCommanded.Modulate = guiColors.FriendColor;
                    }
                    else
                    {
                        inhabited.Visible = true;
                        inhabited.Modulate = guiColors.FriendColor;
                    }
                    break;
                case MapObject.OwnerAlly.Enemy:
                    if (hasActivePeer || state == MapObject.States.Active)
                    {
                        inhabitedCommanded.Visible = true;
                        inhabitedCommanded.Modulate = guiColors.EnemyColor;
                    }
                    else
                    {
                        inhabited.Visible = true;
                        inhabited.Modulate = guiColors.EnemyColor;
                    }
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
                        orbitingCommanded.Visible = true;
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
