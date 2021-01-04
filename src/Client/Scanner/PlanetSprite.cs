using Godot;
using System.Collections.Generic;
using System.Linq;

using CraigStars.Singletons;

namespace CraigStars
{
    public class PlanetSprite : MapObjectSprite
    {
        [Export]
        public GUIColors GUIColors { get; set; } = new GUIColors();

        public enum Orbiting
        {
            None,
            Orbiting,
            OrbitingEnemies,
            OrbitingAlliesAndEnemies
        }

        /// <summary>
        /// Convenience method so the code looks like Fleet.Something instead of MapObject.Something
        /// </summary>
        /// <value></value>
        public Planet Planet
        {
            get => MapObject as Planet;
            set
            {
                MapObject = value;
            }
        }

        public List<FleetSprite> OrbitingFleets { get; set; } = new List<FleetSprite>();

        Sprite known;
        Sprite unknown;
        Sprite inhabited;
        Sprite inhabitedCommanded;
        Sprite orbiting;
        Sprite orbitingCommanded;

        List<Sprite> stateSprites = new List<Sprite>();

        public override void _Ready()
        {
            base._Ready();

            known = GetNode<Sprite>("Sprite/Known");
            unknown = GetNode<Sprite>("Sprite/Unknown");
            inhabited = GetNode<Sprite>("Sprite/Inhabited");
            inhabitedCommanded = GetNode<Sprite>("Sprite/InhabitedCommanded");
            orbiting = GetNode<Sprite>("Sprite/Orbiting");
            orbitingCommanded = GetNode<Sprite>("Sprite/OrbitingActive");

            // create a list of these sprites
            stateSprites.Add(known);
            stateSprites.Add(unknown);
            stateSprites.Add(inhabited);
            stateSprites.Add(inhabitedCommanded);
            stateSprites.Add(orbiting);
            stateSprites.Add(orbitingCommanded);
        }

        public override void _ExitTree()
        {
            base._ExitTree();
        }


        public override List<MapObjectSprite> GetPeers()
        {
            var list = new List<MapObjectSprite>();
            list.AddRange(OrbitingFleets.Where(f => f.OwnedByMe));
            return list;
        }

        public override void UpdateSprite()
        {
            var ownerAllyState = Planet.ReportAge == Planet.Unexplored ? ScannerOwnerAlly.Unknown : ScannerOwnerAlly.Known;
            var state = State;
            var hasActivePeer = HasActivePeer();
            var orbitingState = Orbiting.None;

            // TODO: make this work with multiple types
            if (Planet.OrbitingFleets.Count > 0)
            {
                orbitingState = Orbiting.Orbiting;
            }

            if (Planet.Player != null)
            {
                if (Planet.Player == PlayersManager.Instance.Me)
                {
                    ownerAllyState = ScannerOwnerAlly.Owned;
                }
                else
                {
                    ownerAllyState = ScannerOwnerAlly.Enemy;
                }
            }

            // turn them all off
            stateSprites.ForEach(s => s.Visible = false);

            switch (ownerAllyState)
            {
                case ScannerOwnerAlly.Unknown:
                    unknown.Visible = true;
                    break;
                case ScannerOwnerAlly.Known:
                    if (hasActivePeer || state == ScannerState.Active)
                    {
                        inhabitedCommanded.Visible = true;
                    }
                    else
                    {
                        known.Visible = true;
                    }
                    break;
                case ScannerOwnerAlly.Owned:
                    if (hasActivePeer || state == ScannerState.Active)
                    {
                        inhabitedCommanded.Visible = true;
                        inhabitedCommanded.Modulate = GUIColors.OwnedColor;
                    }
                    else
                    {
                        inhabited.Visible = true;
                        inhabited.Modulate = GUIColors.OwnedColor;
                    }
                    break;
                case ScannerOwnerAlly.Friend:
                    if (hasActivePeer || state == ScannerState.Active)
                    {
                        inhabitedCommanded.Visible = true;
                        inhabitedCommanded.Modulate = GUIColors.FriendColor;
                    }
                    else
                    {
                        inhabited.Visible = true;
                        inhabited.Modulate = GUIColors.FriendColor;
                    }
                    break;
                case ScannerOwnerAlly.Enemy:
                    if (hasActivePeer || state == ScannerState.Active)
                    {
                        inhabitedCommanded.Visible = true;
                        inhabitedCommanded.Modulate = GUIColors.EnemyColor;
                    }
                    else
                    {
                        inhabited.Visible = true;
                        inhabited.Modulate = GUIColors.EnemyColor;
                    }
                    break;
            }

            // turn on the orbiting ring
            switch (orbitingState)
            {
                case Orbiting.Orbiting:
                case Orbiting.OrbitingEnemies:
                case Orbiting.OrbitingAlliesAndEnemies:
                    if (hasActivePeer || state == ScannerState.Active)
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
