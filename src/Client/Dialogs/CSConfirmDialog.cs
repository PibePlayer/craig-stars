using System;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Helper class to make working with ConfirmationDialogs easier to use.
    /// </summary>
    public class CSConfirmDialog : ConfirmationDialog
    {

        Action onOk;
        Action onCancel;

        public override void _Ready()
        {
            PopupExclusive = true;
            Connect("confirmed", this, nameof(OnConfirmed));
            GetCancel().Connect("pressed", this, nameof(OnCancelled));
            GetCloseButton().Connect("pressed", this, nameof(OnCancelled));
        }


        public void Show(string text, Action okAction, Action cancelAction = null)
        {
            onOk = okAction;
            onCancel = cancelAction;

            DialogText = text;
            PopupCentered();
        }

        void OnConfirmed()
        {
            onOk?.Invoke();
        }

        void OnCancelled()
        {
            onCancel?.Invoke();
        }
    }
}