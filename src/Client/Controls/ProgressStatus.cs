using Godot;
using System;

namespace CraigStars.Client
{
    /// <summary>
    /// A simple progress status control with a header and subheader
    /// </summary>
    public class ProgressStatus : VBoxContainer
    {
        public string ProgressLabel
        {
            get => progressLabel;
            set
            {
                progressLabel = value;
                UpdateControls();
            }
        }
        string progressLabel = "Loading";

        public string ProgressSubLabel
        {
            get => progressSubLabel;
            set
            {
                progressSubLabel = value;
                UpdateControls();
            }
        }
        string progressSubLabel;

        public float Progress
        {
            get => progress;
            set
            {
                progress = value;
                UpdateControls();
            }
        }
        float progress;

        ProgressBar progressBar;
        Label label;
        Label subLabel;

        public override void _Ready()
        {
            progressBar = GetNode<ProgressBar>("ProgressBar");
            label = GetNode<Label>("Label");
            subLabel = GetNode<Label>("SubLabel");
        }

        void UpdateControls()
        {
            if (progressBar != null)
            {
                progressBar.Value = progress;
                label.Text = ProgressLabel;
                subLabel.Text = ProgressSubLabel;
            }
        }

    }
}