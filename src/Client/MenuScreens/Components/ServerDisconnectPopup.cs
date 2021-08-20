using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars.Client
{
    public class ServerDisconnectPopup : CenterContainer
    {
        public override void _Ready()
        {
            NetworkClient.Instance.ServerDisconnectedEvent += OnServerDisconnected;
            GetNode<AcceptDialog>("ServerDisconnectedDialog").Connect("confirmed", this, nameof(OnServerDisconnectedDialogConfirmed));
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                NetworkClient.Instance.ServerDisconnectedEvent -= OnServerDisconnected;
            }
        }

        void OnServerDisconnected()
        {
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
            GetTree().ChangeScene("res://src/Client/MainMenu.tscn");
        }
    }
}