using Godot;
using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class TestScene : Node2D
    {
        LineEdit lineEdit;
        Table table;

        public override void _Ready()
        {
            lineEdit = (LineEdit)FindNode("LineEdit");
            table = (Table)FindNode("Table");

            // add two columns
            table.Data.AddColumns("Item", "Quantity", "Col3");

            // add some rows of data
            table.Data.AddRow(new Cost(1, 2, 3, 4), Colors.White, "A", 4, 2);
            table.Data.AddRow(new Cost(2, 3, 4, 5), Colors.White, "B", 3, 1);
            table.Data.AddRow("C", 2, 4);
            table.Data.AddRow("D", 1, 3);

            table.UpdateTable();

            lineEdit.Connect("text_changed", this, nameof(OnSearchLineEditTextChanged));
        }

        void OnSearchLineEditTextChanged(string newText)
        {
            table.Data.Filter(newText);
        }

    }
}