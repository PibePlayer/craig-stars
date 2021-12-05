using System;
using Godot;

namespace CraigStars.Client
{
    /// <summary>
    /// Helper class to make working with ConfirmationDialogs easier to use.
    /// </summary>
    public class CSConfirmDialog : ConfirmationDialog
    {

        private static CSConfirmDialog instance;
        public static CSConfirmDialog Instance
        {
            get
            {
                return instance;
            }
        }

        CSConfirmDialog()
        {
            instance = this;
        }

        static Action onOk;
        static Action onCancel;

        public override void _Ready()
        {
            instance = this;
            PopupExclusive = true;
            Connect("confirmed", this, nameof(OnConfirmed));
            GetCancel().Connect("pressed", this, nameof(OnCancelled));
            GetCloseButton().Connect("pressed", this, nameof(OnCancelled));
        }


        public static void Show(string text, Action okAction, Action cancelAction = null)
        {
            onOk = okAction;
            onCancel = cancelAction;

            instance.DialogText = text;
            instance.PopupCentered();
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