using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class PlanetSprite : MapObjectSprite
    {
        const int MaxPlanetValueRadius = 10;
        const int MaxPopulationRadius = 20;

        [Inject] protected PlanetService planetService;

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
                    countLabel.Text = $"{OrbitingFleets.Sum(fleet => fleet.Fleet.Tokens.Sum(token => token.Quantity))}";
                }
            }
        }

        public bool HasCommandedPeer { get; set; }

        public override bool Commandable { get => true; }

        public List<FleetSprite> OrbitingFleets { get; set; } = new List<FleetSprite>();
        public PlanetSprite PacketTarget { get; set; }

        bool isCommanded;
        Orbiting orbitingState = Orbiting.None;
        ScannerOwnerAlly ownerAllyState = ScannerOwnerAlly.Unknown;

        Sprite known;
        Sprite unknown;
        Sprite inhabited;
        Sprite inhabitedCommanded;
        Sprite orbiting;
        Sprite orbitingCommanded;
        PlanetSpriteMinerals surfaceMinerals;
        PlanetSpriteMinerals mineralConcentration;
        Line2D packetTargetLine;
        Label nameLabel;
        Label countLabel;

        List<Sprite> stateSprites;

        public override void _Ready()
        {
            this.ResolveDependencies();
            base._Ready();

            known = GetNode<Sprite>("Sprite/Known");
            unknown = GetNode<Sprite>("Sprite/Unknown");
            inhabited = GetNode<Sprite>("Sprite/Inhabited");
            inhabitedCommanded = GetNode<Sprite>("Sprite/InhabitedCommanded");
            orbiting = GetNode<Sprite>("Sprite/Orbiting");
            orbitingCommanded = GetNode<Sprite>("Sprite/OrbitingActive");
            packetTargetLine = GetNode<Line2D>("DestinationLine");
            nameLabel = GetNode<Label>("NameLabel");
            countLabel = GetNode<Label>("CountLabel");
            surfaceMinerals = GetNode<PlanetSpriteMinerals>("SurfaceMinerals");
            mineralConcentration = GetNode<PlanetSpriteMinerals>("MineralConcentration");

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

        public override void Returned()
        {
            base.Returned();
            OrbitingFleets.Clear();
            PacketTarget = null;
            HasCommandedPeer = false;
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
                case PlanetViewState.SurfaceMinerals:
                case PlanetViewState.MineralConcentration:
                    break;
                case PlanetViewState.Normal:
                    if (Planet.HasStarbase)
                    {
                        var color = GUIColorsProvider.Colors.StarbaseWithoutDock;
                        if (Planet.Starbase.DockCapacity > 0)
                        {
                            color = GUIColorsProvider.Colors.StarbaseWithDock;
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
                            DrawCircle(new Vector2(0f, -9f), 2.5f, GUIColorsProvider.Colors.MassDriverColor);
                        }
                        else
                        {
                            DrawRect(new Rect2(-1.5f, -7, 3, 3), GUIColorsProvider.Colors.MassDriverColor, true);
                        }
                    }
                    if (Planet.HasStargate)
                    {
                        if (isCommanded)
                        {
                            DrawCircle(new Vector2(-7f, -4f), 2.5f, GUIColorsProvider.Colors.StargateColor);
                        }
                        else
                        {
                            DrawRect(new Rect2(-7f, -5, 3, 3), GUIColorsProvider.Colors.StargateColor, true);
                        }
                    }
                    break;
                case PlanetViewState.Percent:
                    if (Planet.Explored)
                    {
                        var hab = Me.Race.GetPlanetHabitability(Planet.Hab.Value);
                        var circleColor = GUIColorsProvider.Colors.HabitableColor;
                        var outlineColor = GUIColorsProvider.Colors.HabitableOutlineColor;

                        // don't go smaller than 25% of a circle
                        var radius = Mathf.Clamp(MaxPlanetValueRadius * (hab / 100.0f), 2.5f, 10);
                        if (hab < 0)
                        {
                            // this is a red planet, draw it differently
                            int terraformHabValue = hab;
                            Hab terraformedHab = Planet.Hab.Value + Planet.Spec.TerraformAmount;
                            if (terraformedHab != Planet.Hab)
                            {
                                // this is a bad planet but we can terraform it
                                terraformHabValue = Me.Race.GetPlanetHabitability(terraformedHab);
                                if (terraformHabValue > 0)
                                {
                                    radius = Mathf.Clamp(MaxPlanetValueRadius * (terraformHabValue / 100.0f), 2.5f, 10);
                                    circleColor = GUIColorsProvider.Colors.TerraformableColor;
                                    outlineColor = GUIColorsProvider.Colors.TerraformableOutlineColor;
                                }
                                else
                                {
                                    radius = Mathf.Clamp(MaxPlanetValueRadius * (-hab / 45.0f), 2.5f, 10);
                                    circleColor = GUIColorsProvider.Colors.UninhabitableColor;
                                    outlineColor = GUIColorsProvider.Colors.UninhabitableOutlineColor;
                                }
                            }
                            else
                            {
                                radius = Mathf.Clamp(MaxPlanetValueRadius * (-hab / 45.0f), 2.5f, 10);
                                circleColor = GUIColorsProvider.Colors.UninhabitableColor;
                                outlineColor = GUIColorsProvider.Colors.UninhabitableOutlineColor;
                            }
                        }

                        // draw our hab circle
                        DrawCircle(Vector2.Zero, (float)(radius), outlineColor);
                        DrawCircle(Vector2.Zero, (float)(radius * .9f), circleColor);

                        if (Planet.Owned)
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
                    if (Planet.Owned && Planet.Population > 0)
                    {
                        var radius = Math.Max(2, Planet.Population / 1_350_000f * MaxPopulationRadius);
                        var color = OwnedByMe ? GUIColorsProvider.Colors.HabitableColor : PlayerColor;
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
            if (!IsInstanceValid(this) || nameLabel == null || Me == null)
            {
                return;
            }

            nameLabel.Text = Planet.Name;
            nameLabel.Visible = Me.UISettings.ShowPlanetNames;
            var filteredOrbitingFleets = OrbitingFleets.Where(fleet => !fleet.FilteredOut && fleet.Fleet != null).ToList();
            countLabel.Visible = filteredOrbitingFleets.Count > 0 && Me.UISettings.ShowFleetTokenCounts;
            countLabel.Text = $"{filteredOrbitingFleets.Sum(fleet => fleet.Fleet.Tokens.Sum(token => token.Quantity))}";

            ownerAllyState = Planet.ReportAge == MapObject.Unexplored ? ScannerOwnerAlly.Unknown : ScannerOwnerAlly.Known;
            orbitingState = Orbiting.None;
            isCommanded = HasCommandedPeer || State == ScannerState.Commanded;

            if (PacketTarget != null && Planet.PacketTarget != null)
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

            if (filteredOrbitingFleets.Count > 0)
            {
                bool allAllies = true;
                bool allEnemies = true;
                OrbitingFleets.ForEach(fleet =>
                {
                    if (fleet.Fleet.OwnedBy(Me))
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

            if (Planet.Owned)
            {
                if (Planet.OwnedBy(PlayersManager.Me))
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

            if (Planet.Explored
                && planetViewState != PlanetViewState.Normal
                && planetViewState != PlanetViewState.SurfaceMinerals
                && planetViewState != PlanetViewState.MineralConcentration
                )
            {
                // we use the draw
                return;
            }

            surfaceMinerals.Mineral = Planet.Cargo != null ? Planet.Cargo.ToMineral() : new Mineral();
            surfaceMinerals.Scale = Me.UISettings.MineralScale;
            mineralConcentration.Mineral = Planet.MineralConcentration;

            surfaceMinerals.Visible = planetViewState == PlanetViewState.SurfaceMinerals;
            mineralConcentration.Visible = planetViewState == PlanetViewState.MineralConcentration;

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
                        inhabitedCommanded.Modulate = GUIColorsProvider.Colors.OwnedColor;
                    }
                    else
                    {
                        inhabited.Visible = true;
                        inhabited.Modulate = GUIColorsProvider.Colors.OwnedColor;
                    }
                    break;
                case ScannerOwnerAlly.Friend:
                    if (isCommanded)
                    {
                        inhabitedCommanded.Visible = true;
                        inhabitedCommanded.Modulate = PlayerColor;
                    }
                    else
                    {
                        inhabited.Visible = true;
                        inhabited.Modulate = PlayerColor;
                    }
                    break;
                case ScannerOwnerAlly.Enemy:
                    if (isCommanded)
                    {
                        inhabitedCommanded.Visible = true;
                        inhabitedCommanded.Modulate = PlayerColor;
                    }
                    else
                    {
                        inhabited.Visible = true;
                        inhabited.Modulate = PlayerColor;
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
                    orbitingCommanded.Modulate = GUIColorsProvider.Colors.EnemyColor;
                    orbiting.Modulate = GUIColorsProvider.Colors.EnemyColor;
                    break;
                case Orbiting.OrbitingAlliesAndEnemies:
                    orbitingCommanded.Modulate = GUIColorsProvider.Colors.FriendAndEnemyColor;
                    orbiting.Modulate = GUIColorsProvider.Colors.FriendAndEnemyColor;
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
