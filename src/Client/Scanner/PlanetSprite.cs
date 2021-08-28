using Godot;
using System.Collections.Generic;
using System.Linq;

using CraigStars.Singletons;
using System;

namespace CraigStars.Client
{
    public class PlanetSprite : MapObjectSprite
    {
        const int MaxPlanetValueRadius = 10;
        const int MaxPopulationRadius = 20;

        PlanetService planetService = new();

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
                if (value != null && nameLabel != null)
                {
                    nameLabel.Text = Planet.Name;
                }
            }
        }

        public override bool Commandable { get => true; }

        public List<FleetSprite> OrbitingFleets { get; set; } = new List<FleetSprite>();
        public PlanetSprite PacketTarget { get; set; }

        bool hasActivePeer;
        bool isCommanded;
        Orbiting orbitingState = Orbiting.None;
        ScannerOwnerAlly ownerAllyState = ScannerOwnerAlly.Unknown;

        Sprite known;
        Sprite unknown;
        Sprite inhabited;
        Sprite inhabitedCommanded;
        Sprite orbiting;
        Sprite orbitingCommanded;
        Line2D packetTargetLine;
        Label nameLabel;

        List<Sprite> stateSprites;

        public override void _Ready()
        {
            base._Ready();

            known = GetNode<Sprite>("Sprite/Known");
            unknown = GetNode<Sprite>("Sprite/Unknown");
            inhabited = GetNode<Sprite>("Sprite/Inhabited");
            inhabitedCommanded = GetNode<Sprite>("Sprite/InhabitedCommanded");
            orbiting = GetNode<Sprite>("Sprite/Orbiting");
            orbitingCommanded = GetNode<Sprite>("Sprite/OrbitingActive");
            packetTargetLine = GetNode<Line2D>("DestinationLine");
            nameLabel = GetNode<Label>("NameLabel");

            // create a list of these sprites
            stateSprites = new List<Sprite>() {
                known,
                unknown,
                inhabited,
                inhabitedCommanded,
                orbiting,
                orbitingCommanded
            };

            UpdateSprite();
        }

        public override List<MapObjectSprite> GetPeers()
        {
            var list = new List<MapObjectSprite>();
            list.AddRange(OrbitingFleets.Where(f => f.OwnedByMe));
            return list;
        }

        public override void _Draw()
        {
            if (!IsInstanceValid(this))
            {
                return;
            }

            switch (PlayersManager.Me.UISettings.PlanetViewState)
            {
                // just use sprites
                case PlanetViewState.None:
                    break;
                case PlanetViewState.Normal:
                    if (Planet.HasStarbase)
                    {
                        var color = GUIColors.StarbaseWithoutDock;
                        if (Planet.Starbase.DockCapacity > 0)
                        {
                            color = GUIColors.StarbaseWithDock;
                        }
                        if (isCommanded)
                        {
                            DrawCircle(new Vector2(7f, -4f), 2.5f, color);
                        }
                        else
                        {
                            DrawRect(new Rect2(4f, -5, 3, 3), color, true);
                        }
                    }
                    if (Planet.HasMassDriver)
                    {
                        if (isCommanded)
                        {
                            DrawCircle(new Vector2(0f, -9f), 2.5f, GUIColors.MassDriverColor);
                        }
                        else
                        {
                            DrawRect(new Rect2(-1.5f, -7, 3, 3), GUIColors.MassDriverColor, true);
                        }
                    }
                    if (Planet.HasStargate)
                    {
                        if (isCommanded)
                        {
                            DrawCircle(new Vector2(-7f, -4f), 2.5f, GUIColors.StargateColor);
                        }
                        else
                        {
                            DrawRect(new Rect2(-7f, -5, 3, 3), GUIColors.StargateColor, true);
                        }
                    }
                    break;
                case PlanetViewState.Percent:
                    if (Planet.Explored)
                    {
                        var hab = Me.Race.GetPlanetHabitability(Planet.Hab.Value);
                        var circleColor = GUIColors.HabitableColor;
                        var outlineColor = GUIColors.HabitableOutlineColor;

                        // don't go smaller than 25% of a circle
                        var radius = Mathf.Clamp(MaxPlanetValueRadius * (hab / 100.0f), 2.5f, 10);
                        if (hab < 0)
                        {
                            // this is a red planet, draw it differently
                            int terraformHabValue = hab;
                            Hab terraformedHab = Planet.Hab.Value + planetService.GetTerraformAmount(Planet, Me);
                            if (terraformedHab != Planet.Hab)
                            {
                                // this is a bad planet but we can terraform it
                                terraformHabValue = Me.Race.GetPlanetHabitability(terraformedHab);
                                if (terraformHabValue > 0)
                                {
                                    radius = Mathf.Clamp(MaxPlanetValueRadius * (terraformHabValue / 100.0f), 2.5f, 10);
                                    circleColor = GUIColors.TerraformableColor;
                                    outlineColor = GUIColors.TerraformableOutlineColor;
                                }
                                else
                                {
                                    radius = Mathf.Clamp(MaxPlanetValueRadius * (-hab / 45.0f), 2.5f, 10);
                                    circleColor = GUIColors.UninhabitableColor;
                                    outlineColor = GUIColors.UninhabitableOutlineColor;
                                }
                            }
                            else
                            {
                                radius = Mathf.Clamp(MaxPlanetValueRadius * (-hab / 45.0f), 2.5f, 10);
                                circleColor = GUIColors.UninhabitableColor;
                                outlineColor = GUIColors.UninhabitableOutlineColor;
                            }
                        }

                        // draw our hab circle
                        DrawCircle(Vector2.Zero, (float)(radius), outlineColor);
                        DrawCircle(Vector2.Zero, (float)(radius * .9f), circleColor);

                        if (Planet.Owner != null)
                        {
                            // draw a blue flag for our planet, red for other player's planet
                            var color = Planet.OwnedBy(Me) ? Colors.Blue : Colors.Red;
                            DrawRect(new Rect2(0, -20, 9, 8), color, true);
                            DrawRect(new Rect2(0, -20, 9, 8), Colors.Black, false, 2);
                            DrawLine(Vector2.Zero, new Vector2(0, -(MaxPlanetValueRadius + 2)), color, 2);
                        }
                    }
                    break;
                case PlanetViewState.Population:
                    if (Planet.Owner != null && Planet.Population > 0)
                    {
                        var radius = Math.Max(2, Planet.Population / 1_350_000f * MaxPopulationRadius);
                        var color = OwnedByMe ? GUIColors.HabitableColor : Planet.Owner.Color;
                        DrawCircle(Vector2.Zero, radius, color);
                    }
                    else
                    {
                        known.Visible = Planet.Explored;
                        unknown.Visible = !Planet.Explored;
                    }
                    break;
            }
        }

        public override void UpdateSprite()
        {
            if (!IsInstanceValid(this) || nameLabel == null)
            {
                return;
            }

            nameLabel.Text = Planet.Name;
            nameLabel.Visible = Me.UISettings.ShowPlanetNames;

            ownerAllyState = Planet.ReportAge == MapObject.Unexplored ? ScannerOwnerAlly.Unknown : ScannerOwnerAlly.Known;
            hasActivePeer = HasActivePeer();
            orbitingState = Orbiting.None;
            isCommanded = hasActivePeer || State == ScannerState.Commanded;

            if (PacketTarget != null)
            {
                packetTargetLine.Points = new Vector2[] {
                    new Vector2(),
                    Planet.PacketTarget.Position - Planet.Position
                };
            }
            else
            {
                packetTargetLine.Points = new Vector2[] { Position };
            }

            // TODO: make this work with multiple types
            if (Planet.OrbitingFleets.Count > 0)
            {
                bool allAllies = true;
                bool allEnemies = true;
                Planet.OrbitingFleets.ForEach(fleet =>
                {
                    if (fleet.OwnedBy(Me))
                    {
                        allEnemies = false;
                    }
                    else
                    {
                        allAllies = false;
                    }
                });
                if (allAllies)
                {
                    orbitingState = Orbiting.Orbiting;
                }
                else if (allEnemies)
                {
                    orbitingState = Orbiting.OrbitingEnemies;
                }
                else
                {
                    orbitingState = Orbiting.OrbitingAlliesAndEnemies;
                }
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

            var planetViewState = PlayersManager.Me.UISettings.PlanetViewState;

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
                    if (isCommanded)
                    {
                        inhabitedCommanded.Visible = true;
                    }
                    else
                    {
                        known.Visible = true;
                    }
                    break;
                case ScannerOwnerAlly.Owned:
                    if (isCommanded)
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
                    if (isCommanded)
                    {
                        inhabitedCommanded.Visible = true;
                        inhabitedCommanded.Modulate = Planet.Owner.Color;
                    }
                    else
                    {
                        inhabited.Visible = true;
                        inhabited.Modulate = Planet.Owner.Color;
                    }
                    break;
                case ScannerOwnerAlly.Enemy:
                    if (isCommanded)
                    {
                        inhabitedCommanded.Visible = true;
                        inhabitedCommanded.Modulate = Planet.Owner.Color;
                    }
                    else
                    {
                        inhabited.Visible = true;
                        inhabited.Modulate = Planet.Owner.Color;
                    }
                    break;
            }

            // turn on the orbiting ring
            switch (orbitingState)
            {
                case Orbiting.Orbiting:
                    orbitingCommanded.Modulate = Colors.White;
                    orbiting.Modulate = Colors.White;
                    break;
                case Orbiting.OrbitingEnemies:
                    orbitingCommanded.Modulate = GUIColors.EnemyColor;
                    orbiting.Modulate = GUIColors.EnemyColor;
                    break;
                case Orbiting.OrbitingAlliesAndEnemies:
                    orbitingCommanded.Modulate = GUIColors.FriendAndEnemyColor;
                    orbiting.Modulate = GUIColors.FriendAndEnemyColor;
                    break;
            }

            if (orbitingState != Orbiting.None)
            {
                if (isCommanded)
                {
                    orbitingCommanded.Visible = true;
                }
                else
                {
                    orbiting.Visible = true;
                }

            }

        }
    }
}
