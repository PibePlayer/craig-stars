using CraigStarsTable;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars.Client
{

    public class PlayerRelationsDialog : GameViewDialog
    {
        PlayersTable playersTable;
        CheckBox friendCheckBox;
        CheckBox neutralCheckBox;
        CheckBox enemyCheckBox;

        int currentPlayer = -1;

        List<PlayerRelationship> PlayerRelations { get; set; } = new();

        public override void _Ready()
        {
            base._Ready();

            playersTable = GetNode<PlayersTable>("MarginContainer/VBoxContainer/ContentContainer/HBoxContainer/PlayersVBoxContainer/PlayersTable");
            friendCheckBox = GetNode<CheckBox>("MarginContainer/VBoxContainer/ContentContainer/HBoxContainer/Relation/VBoxContainer/FriendCheckBox");
            neutralCheckBox = GetNode<CheckBox>("MarginContainer/VBoxContainer/ContentContainer/HBoxContainer/Relation/VBoxContainer/NeutralCheckBox");
            enemyCheckBox = GetNode<CheckBox>("MarginContainer/VBoxContainer/ContentContainer/HBoxContainer/Relation/VBoxContainer/EnemyCheckBox");

            friendCheckBox.Connect("pressed", this, nameof(OnRelationCheckBoxPressed), new Godot.Collections.Array() { PlayerRelation.Friend });
            neutralCheckBox.Connect("pressed", this, nameof(OnRelationCheckBoxPressed), new Godot.Collections.Array() { PlayerRelation.Neutral });
            enemyCheckBox.Connect("pressed", this, nameof(OnRelationCheckBoxPressed), new Godot.Collections.Array() { PlayerRelation.Enemy });

            playersTable.Data.Clear();
            playersTable.Data.AddColumns("Name", "Race");

            playersTable.RowSelectedEvent += OnPlayerStatusRowSelected;

        }

        void OnPlayerStatusRowSelected(int rowIndex, int colIndex, Cell cell, PublicPlayerInfo player)
        {
            // default to neutral
            currentPlayer = -1;
            neutralCheckBox.Pressed = true;
            if (player.Num >= 0 && player.Num < PlayerRelations.Count)
            {
                currentPlayer = player.Num;
                var relation = PlayerRelations[player.Num];

                friendCheckBox.Pressed = relation.Relation == PlayerRelation.Friend;
                neutralCheckBox.Pressed = relation.Relation == PlayerRelation.Neutral;
                enemyCheckBox.Pressed = relation.Relation == PlayerRelation.Enemy;
            }
        }

        void OnRelationCheckBoxPressed(PlayerRelation relation)
        {
            if (currentPlayer >= 0 && currentPlayer < PlayerRelations.Count)
            {
                PlayerRelations[currentPlayer].Relation = relation;
            }
        }

        protected override void OnOk()
        {
            base.OnOk();
            Me.PlayerRelations = new(PlayerRelations);
            Me.Dirty = true;
            EventManager.PublishPlayerDirtyEvent();
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            if (IsVisibleInTree())
            {

                PlayerRelations = new List<PlayerRelationship>(Me.PlayerRelations);

                // draw the table
                var otherPlayers = GameInfo.Players.Where(p => p.Num != Me.Num).ToList();
                var firstPlayer = otherPlayers.FirstOrDefault();
                bool resetTable = playersTable.Data.Rows.Count() != otherPlayers.Count;
                playersTable.Data.ClearRows();
                otherPlayers.ForEach(player =>
                {
                    playersTable.Data.AddRowAdvanced(metadata: player, color: Colors.White, italic: false,

                        player.Name,
                        player.RacePluralName
                    );
                });

                if (resetTable)
                {
                    playersTable.ResetTable();
                }
                else
                {
                    playersTable.UpdateRows();
                }
            }

        }



    }
}