using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client
{
    public class PlanetTile : Control, ITileContent
    {
        public PlanetSprite CommandedPlanet { get; set; }
        public Player Me { get => PlayersManager.Me; }

        public event UpdateTitleAction UpdateTitleEvent;
        public event UpdateVisibilityAction UpdateVisibilityEvent;

        public override void _Ready()
        {
            EventManager.MapObjectCommandedEvent += OnMapObjectCommanded;
            EventManager.TurnPassedEvent += OnTurnPassed;
        }

        public override void _ExitTree()
        {
            EventManager.MapObjectCommandedEvent -= OnMapObjectCommanded;
            EventManager.TurnPassedEvent -= OnTurnPassed;
        }

        protected void UpdateTitle(string title)
        {
            UpdateTitleEvent?.Invoke(title);
        }

        protected virtual void OnTurnPassed(PublicGameInfo gameInfo, Player player)
        {
            CommandedPlanet = null;
            UpdateControls();
        }

        protected virtual void OnMapObjectCommanded(MapObjectSprite mapObject)
        {
            CommandedPlanet = mapObject as PlanetSprite;
            Visible = CommandedPlanet != null;
            UpdateVisibilityEvent?.Invoke(Visible);
            UpdateControls();
        }

        protected virtual void UpdateControls()
        {
        }
    }
}
