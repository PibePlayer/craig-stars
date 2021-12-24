using System;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class ScannerTooltip : CSTooltip
    {
        RichTextLabel tipRichTextLabel;

        public override void _Ready()
        {
            tipRichTextLabel = GetNode<RichTextLabel>("MarginContainer/TipRichTextLabel");
        }

        protected override void UpdateControls()
        {
            if (Planet != null)
            {
                if (Me.Race.Spec.InnateScanner)
                {
                    tipRichTextLabel.BbcodeText += "Your race cannot build planetary scanners. Your starbases have an innate scanning ability whose distance is equal to the square root of one tenth the starbase population.";
                }
                else
                {
                }
            }
        }
    }
}