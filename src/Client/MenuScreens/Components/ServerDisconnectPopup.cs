using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    public class ServerDisconnectPopup : CenterContainer
    {
        public override void _Ready()
        {
            Signals.ServerDisconnectedEvent += OnServerDisconnected;
            GetNode<AcceptDialog>("ServerDisconnectedDialog").Connect("confirmed", this, nameof(OnServerDisconnectedDialogConfirmed));
        }

        public override void _ExitTree()
        {
            Signals.ServerDisconnectedEvent -= OnServerDisconnected;
        }

        void OnServerDisconnected()
        {
            PlayersManager.Instance.Reset();

            if (GetParent() != null)
            {
                foreach (var child in GetParent().GetChildren())
                {
                    if (child != this)
                    {
                        Color color = ((CanvasItem)GetParent().GetChild(0)).Modulate;
                        color.a = .5f;
                        ((CanvasItem)GetParent().GetChild(0)).Modulate = color;
                    }
                }
            }
            GetNode<AcceptDialog>("ServerDisconnectedDialog").Show();
        }

        void OnServerDisconnectedDialogConfirmed()
        {
            GetTree().ChangeScene("res://src/Client/Main.tscn");
        }
    }
}