using System.Threading.Tasks;
using CraigStars.Singletons;
using Godot;

namespace CraigStars
{
    public class DedicatedServer : Node
    {
        public int Port { get; set; } = 3000;
        public string GameName { get; set; }
        public int Year { get; set; } = -1;

        public override void _Ready()
        {
            base._Ready();

            // if we didn't specify a year, get the latest
            if (Year == -1)
            {
                var years = GamesManager.Instance.GetGameSaveYears(GameName);
                Year = years[years.Count - 1];
            }
            ServerManager.Instance.HostGame(Port, GameName, Year);
        }

        public override void _Notification(int what)
        {
            if (what == MainLoop.NotificationWmQuitRequest)
            {
                ServerManager.Instance.ExitGame();
                GetTree().Quit(); // default behavior
            }
        }

    }
}