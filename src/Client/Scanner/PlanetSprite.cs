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

        bool hasActivePeer;
        bool isActive;
        Orbiting orbitingState = Orbiting.None;
        ScannerOwnerAlly ownerAllyState = ScannerOwnerAlly.Unknown;

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

        public override void _Draw()
        {
            switch (PlayersManager.Me.PlanetViewState)
            {
                // just use sprites
                case PlanetViewState.None:
                    break;
                case PlanetViewState.Normal:
                    if (Planet.HasStarbase)
                    {
                        if (isActive)
                        {
                            DrawCircle(new Vector2(7f, -4f), 2.5f, Colors.Yellow);
                        }
                        else
                        {
                            DrawRect(new Rect2(3f, -4, 3, 3), Colors.Yellow, true);
                        }
                    }
                    break;
                case PlanetViewState.Percent:
                    if (Planet.Explored)
                    {
                        var hab = Me.Race.GetPlanetHabitability(Planet.Hab.Value);
                        if (hab > 0)
                        {
                            // don't go smaller than 25%
                            var radius = Mathf.Clamp(10 * (hab / 100.0f), 2.5f, 10);
                            DrawCircle(Vector2.Zero, (float)(10 * (hab / 100.0)), GUIColors.HabitableOutlineColor);
                            DrawCircle(Vector2.Zero, (float)(8 * (hab / 100.0)), GUIColors.HabitableColor);
                        }
                        else if (hab < 0)
                        {
                            // don't go smaller than 25%
                            var radius = Mathf.Clamp(10 * (hab / 45.0f), 2.5f, 10);
                            DrawCircle(Vector2.Zero, radius, GUIColors.UninhabitableOutlineColor);
                            DrawCircle(Vector2.Zero, radius - 1, GUIColors.UninhabitableColor);
                        }

                        if (Planet.Owner != null)
                        {
                            // draw a blue flag for our planet, red for other player's planet
                            var color = Planet.OwnedBy(Me) ? Colors.Blue : Colors.Red;
                            DrawRect(new Rect2(0, -20, 9, 8), color, true);
                            DrawRect(new Rect2(0, -20, 9, 8), Colors.Black, false, 2);
                            DrawLine(Vector2.Zero, new Vector2(0, -12), color, 2);
                        }
                    }
                    break;
            }
        }

        public override void UpdateSprite()
        {

            ownerAllyState = Planet.ReportAge == Planet.Unexplored ? ScannerOwnerAlly.Unknown : ScannerOwnerAlly.Known;
            hasActivePeer = HasActivePeer();
            orbitingState = Orbiting.None;
            isActive = hasActivePeer || State == ScannerState.Commanded;

            // TODO: make this work with multiple types
            if (Planet.OrbitingFleets.Count > 0)
            {
                orbitingState = Orbiting.Orbiting;
            }

            if (Planet.Owner != null)
            {
                if (Planet.Owner == PlayersManager.Me)
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

            // do any custom drawing
            Update();

            var planetViewState = PlayersManager.Me.PlanetViewState;

            if (planetViewState == PlanetViewState.None)
            {
                // boring squares
                unknown.Visible = true;
                return;
            }

            if (Planet.Explored && planetViewState != PlanetViewState.Normal)
            {
                // we use the draw
                return;
            }

            switch (ownerAllyState)
            {
                case ScannerOwnerAlly.Unknown:
                    unknown.Visible = true;
                    break;
                case ScannerOwnerAlly.Known:
                    if (isActive)
                    {
                        inhabitedCommanded.Visible = true;
                    }
                    else
                    {
                        known.Visible = true;
                    }
                    break;
                case ScannerOwnerAlly.Owned:
                    if (isActive)
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
                    if (isActive)
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
                    if (isActive)
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
                    if (isActive)
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
