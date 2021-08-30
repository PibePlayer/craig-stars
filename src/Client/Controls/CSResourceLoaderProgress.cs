using CraigStars.Singletons;
using CraigStarsTable;
using Godot;
using System;
namespace CraigStars.Client
{
    public class CSResourceLoaderProgress : MarginContainer
    {
        Label progressLabel;
        ProgressBar progressBar;

        public override void _Ready()
        {
            progressLabel = GetNode<Label>("VBoxContainer/ProgressLabel");
            progressBar = GetNode<ProgressBar>("VBoxContainer/ProgressBar");
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            if (CSResourceLoader.TotalResources > 0 && CSResourceLoader.Loaded < CSResourceLoader.TotalResources)
            {
                progressLabel.Text = "Loading CraigStars! Resources";
                progressBar.Value = (float)CSResourceLoader.Loaded / CSResourceLoader.TotalResources * 100;
            }
            else if (CSTableResourceLoader.TotalResources > 0 && CSTableResourceLoader.Loaded < CSTableResourceLoader.TotalResources)
            {
                progressLabel.Text = "Loading CraigStars! Table Resources";
                progressBar.Value = (float)CSTableResourceLoader.Loaded / CSTableResourceLoader.TotalResources * 100;
            }
            else
            {
                SetProcess(false);
                progressBar.Visible = progressLabel.Visible = false;
            }
        }

    }
}