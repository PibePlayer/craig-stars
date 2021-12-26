using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CraigStars.Singletons;
using CraigStars.Utils;
using CraigStarsTable;
using Godot;

namespace CraigStars.Client
{
    public class LoadGameMenu : MarginContainer
    {
        static CSLog log = LogProvider.GetLogger(typeof(LoadGameMenu));

        List<string> gameNames;
        ItemList itemList;
        PublicGameInfoDetail publicGameInfoDetail;

        public override void _Ready()
        {
            base._Ready();
            ((Button)FindNode("BackButton")).Connect("pressed", this, nameof(OnBackPressed));

            itemList = GetNode<ItemList>("VBoxContainer/CenterContainer/Panel/MarginContainer/HBoxContainer/MenuButtons/HBoxContainer/VBoxContainerList/ItemList");
            publicGameInfoDetail = GetNode<PublicGameInfoDetail>("VBoxContainer/CenterContainer/Panel/MarginContainer/HBoxContainer/MenuButtons/HBoxContainer/PublicGameInfoDetail");

            itemList.Connect("item_selected", this, nameof(OnGameSelected));

            publicGameInfoDetail.OnDeleted += OnGameDeleted;
            UpdateItemList();
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                publicGameInfoDetail.OnDeleted -= OnGameDeleted;
            }
        }

        void OnGameSelected(int index)
        {
            // var gameInfo = GamesManager.Instance.LoadPlayerGameInfo(gameNames[index]);
            var playerSaves = GamesManager.Instance.LoadPlayerSaves(gameNames[index]);
            publicGameInfoDetail.PlayerSaves = playerSaves;
            publicGameInfoDetail.UpdateControls();
        }

        void OnGameDeleted(PublicGameInfoDetail gameInfoDetail)
        {
            UpdateItemList();
        }

        void UpdateItemList()
        {
            itemList.Clear();
            gameNames = GamesManager.Instance.GetSavedGames();
            if (gameNames != null && gameNames.Count > 0)
            {
                publicGameInfoDetail.Visible = true;
                gameNames.ForEach(gameName => itemList.AddItem(gameName));
                itemList.Select(0);
                OnGameSelected(0);
            }
            else
            {
                publicGameInfoDetail.Visible = false;
            }
        }

        void OnBackPressed()
        {
            GetTree().ChangeScene("res://src/Client/MainMenu.tscn");
        }
    }
}