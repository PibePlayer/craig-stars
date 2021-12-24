using System;
using CraigStarsTable;
using Godot;

namespace CraigStars.Client
{
    [Tool]
    public class PublicGameInfosTable : Table<PublicGameInfo>
    {
        public override void _Ready()
        {

            var columnHeaderScene = ResourceLoader.Load<PackedScene>("res://addons/CraigStarsComponents/src/PublicGameInfosColumnHeader.tscn");
            ColumnHeaderProvider = (col) => CSTableNodePool.Get<PublicGameInfosColumnHeader>(columnHeaderScene);
            base._Ready();
        }
    }
}