using System;
using Godot;

namespace CraigStars.Client
{
    public class LineEditDialog : GameViewDialog
    {
        public string Text
        {
            get => lineEdit?.Text;
            set
            {
                if (lineEdit != null)
                {
                    lineEdit.Text = value;
                }
            }
        }

        Action<string> onOk;


        LineEdit lineEdit;
        public override void _Ready()
        {
            base._Ready();
            lineEdit = GetNode<LineEdit>("MarginContainer/VBoxContainer/ContentContainer/LineEdit");
            lineEdit.Connect("text_entered", this, nameof(OnLineEditTextEntered));
        }

        void OnLineEditTextEntered(string newText)
        {
            OnOk();
        }

        protected override void OnOk()
        {
            base.OnOk();
            onOk?.Invoke(lineEdit.Text);
        }

        /// <summary>
        /// Override 
        /// </summary>
        /// <param name="text"></param>
        public void PopupCentered(string text, Action<string> onOk)
        {
            // setup the dialog and show it
            this.onOk = onOk;
            Text = text;
            PopupCentered();

            CallDeferred(nameof(FocusOnShow));
        }

        public void FocusOnShow()
        {
            lineEdit.GrabFocus();
            lineEdit.SelectAll();
        }

    }
}