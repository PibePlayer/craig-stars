using CraigStars.Singletons;
using Godot;

namespace CraigStars
{
    public class PlanetTile : Control, ITileContent
    {
        public PlanetSprite CommandedPlanet { get; set; }
        public Player Me { get => PlayersManager.Me; }

        public event UpdateTitleAction UpdateTitle;
        public event UpdateVisibilityAction UpdateVisibility;

        public override void _Ready()
        {
            Signals.MapObjectCommandedEvent += OnMapObjectCommanded;
            Signals.TurnPassedEvent += OnTurnPassed;
        }

        public override void _ExitTree()
        {
            Signals.MapObjectCommandedEvent -= OnMapObjectCommanded;
            Signals.TurnPassedEvent -= OnTurnPassed;
        }

        protected virtual void OnTurnPassed(PublicGameInfo gameInfo)
        {
            CommandedPlanet = null;
            UpdateControls();
        }

        protected virtual void OnMapObjectCommanded(MapObjectSprite mapObject)
        {
            CommandedPlanet = mapObject as PlanetSprite;
            Visible = CommandedPlanet != null;
            UpdateVisibility?.Invoke(Visible);
            UpdateControls();
        }

        protected virtual void UpdateControls()
        {
        }
    }
}
