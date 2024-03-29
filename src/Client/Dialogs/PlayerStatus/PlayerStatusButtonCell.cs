using System;
using System.Linq;
using CraigStars.Singletons;
using CraigStarsTable;
using Godot;

namespace CraigStars.Client
{
    public class PlayerStatusButtonCell : CSButtonCell<PublicPlayerInfo>
    {
        Player Me { get => PlayersManager.Me; }
        PublicGameInfo GameInfo { get => PlayersManager.GameInfo; }

        protected Label label;

        public PlayerStatusButtonCell() { }
        public PlayerStatusButtonCell(Column<PublicPlayerInfo> col, Cell cell, Row<PublicPlayerInfo> row, Action<CSButtonCell<PublicPlayerInfo>> onPressed) : base(col, cell, row, onPressed)
        {
        }

        public override void _Ready()
        {
            base._Ready();
            label = new Label()
            {
                SizeFlagsHorizontal = (int)SizeFlags.ExpandFill,
                SizeFlagsVertical = (int)SizeFlags.Expand | (int)SizeFlags.ShrinkCenter,
            };
            AddChild(label);
            UpdateCell();
        }

        /// <summary>
        /// Update the player status cell with a button if we can unsubmit/submit/play
        /// or with text if this is an AI player or some other player
        /// 
        /// TODO: This code is awful. It needs to be cleaned up and unit tested
        /// </summary>
        public override void UpdateCell()
        {
            base.UpdateCell();

            // setup a label as well as a button
            if (label != null)
            {
                PublicPlayerInfo player = Row.Metadata;

                if (Me != null && Me.Num == player.Num && !Me.SubmittedTurn)
                {
                    Cell.Text = "Playing";
                }
                else if (player.SubmittedTurn)
                {
                    Cell.Text = "Submitted";
                }
                else
                {
                    Cell.Text = "Waiting to Submit";
                }

                label.Text = Cell.Text;
                label.Align = Column.Align;

                if (Row.Italic || Cell.Italic)
                {
                    var font = new DynamicFont()
                    {
                        FontData = italicFont,
                        Size = 14,
                    };
                    label.AddFontOverride("font", font);
                }

                // use a cell color override or a row color override
                if (Cell.Color != Colors.White)
                {
                    label.Modulate = Cell.Color;
                }
                else if (Row.Color.HasValue && Row.Color != Colors.White)
                {
                    label.Modulate = Row.Color.Value;
                }
                else
                {
                    label.Modulate = Colors.White;
                }

                // just show a label if this is an AI player or we are generating a turn
                if ((GameInfo.State == GameState.GeneratingTurn || GameInfo.State == GameState.Setup) || Row.Metadata.AIControlled)
                {
                    button.Visible = false;
                    label.Visible = true;
                }
                else
                {
                    if (Me == null || Me.Num != Row.Metadata.Num || (Me != null && Me.SubmittedTurn))
                    {
                        label.Visible = false;
                        button.Visible = true;

                        if (player.SubmittedTurn)
                        {
                            button.Text = "Unsubmit Turn";
                        }
                        else
                        {
                            button.Text = "Play Turn";
                        }
                    }
                    else
                    {
                        button.Visible = false;
                        label.Visible = true;
                    }
                }
            }
        }

    }
}