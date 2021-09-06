using CraigStarsTable;
using Godot;
using System;

namespace CraigStars.Client
{
    [Tool]
    public class PlayerSavesTable : Table<PlayerSave>
    {
        public override void _Ready()
        {
            var columnHeaderScene = ResourceLoader.Load<PackedScene>("res://addons/CraigStarsComponents/src/PlayerSavesColumnHeader.tscn");
            ColumnHeaderProvider = (col) => CSTableNodePool.Get<PlayerSavesColumnHeader>(columnHeaderScene);
            base._Ready();
        }
    }
}