using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class PlanetTile : Control
    {
        [Inject] protected PlanetService planetService;
        public PlanetSprite CommandedPlanet { get; set; }
        public Player Me { get => PlayersManager.Me; }
        public PublicGameInfo GameInfo { get => PlayersManager.GameInfo; }

        Label titleLabel;

        public override void _Ready()
        {
            this.ResolveDependencies();
            base._Ready();

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
