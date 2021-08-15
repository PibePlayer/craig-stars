using CraigStarsTable;
using Godot;
using System;

namespace CraigStars
{
    [Tool]
    public class PlayersTable : Table<PublicPlayerInfo>
    {
        public PlayersTable() : base()
        {
            CellControlScript = "res://addons/CraigStarsComponents/src/PlayersTableLabelCell.cs";
            ColumnHeaderScene = "res://addons/CSTable/src/Table/ColumnHeader.tscn";
        }

    }
}