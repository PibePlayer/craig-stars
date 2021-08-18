using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client
{
    public class PlanetTile : Control
    {
        public PlanetSprite CommandedPlanet { get; set; }
        public Player Me { get => PlayersManager.Me; }

        Label titleLabel;

        public override void _Ready()
        {
            titleLabel = (Label)FindNode("TitleLabel");
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
            titleLabel.Text = title;
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
            UpdateControls();
        }

        protected virtual void UpdateControls()
        {
        }
    }
}
