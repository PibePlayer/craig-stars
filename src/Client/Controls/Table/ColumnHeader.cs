using Godot;
using System;


namespace CraigStars
{
    /// <summary>
    /// The default column header for a table
    /// </summary>
    [Tool]
    public class ColumnHeader : Control, IColumnHeader
    {
        public event Action<ColumnHeader> SortEvent;

        public Column Column
        {
            get => column;
            set
            {
                column = value;
                UpdateLabel();
            }
        }
        Column column;

        public SortDirection SortDirection
        {
            get => sortDirection;
            set
            {
                sortDirection = value;
                UpdateSort();
            }
        }
        SortDirection sortDirection = SortDirection.None;

        Label label;
        TextureRect sortButton;

        public override void _Ready()
        {
            label = GetNode<Label>("HBoxContainer/Label");
            sortButton = GetNode<TextureRect>("HBoxContainer/SortButton");
            Connect("gui_input", this, nameof(OnSortButtonGuiInput));
            Connect("mouse_entered", this, nameof(OnMouseEntered));
            Connect("mouse_exited", this, nameof(OnMouseExited));
            UpdateSort();
            UpdateLabel();
        }

        void OnSortButtonGuiInput(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_select"))
            {
                SortEvent?.Invoke(this);
            }
        }

        void OnMouseEntered()
        {
            Modulate = Colors.DarkGray;
        }

        void OnMouseExited()
        {
            Modulate = Colors.White;
        }

        protected void UpdateLabel()
        {
            if (label != null)
            {
                label.Text = Column != null ? Column.Name : "";
            }
        }

        protected virtual void UpdateSort()
        {
            if (sortButton != null)
            {
                if (SortDirection == SortDirection.None)
                {
                    sortButton.Visible = false;
                }
                else
                {
                    sortButton.Visible = true;
                    sortButton.FlipV = (sortDirection == SortDirection.Descending);
                }
            }
        }
    }
}