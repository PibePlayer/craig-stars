using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client
{
    public class PlanetTile : Control
    {
        protected PlanetService planetService = new();
        public PlanetSprite CommandedPlanet { get; set; }
        public Player Me { get => PlayersManager.Me; }

        Label titleLabel;

        public override void _Ready()
        {
            titleLabel = (Label)FindNode("TitleLabel");
            EventManager.MapObjectCommandedEvent += OnMapObjectCommanded;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                EventManager.MapObjectCommandedEvent -= OnMapObjectCommanded;
            }
        }

        protected void UpdateTitle(string title)
        {
            titleLabel.Text = title;
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
