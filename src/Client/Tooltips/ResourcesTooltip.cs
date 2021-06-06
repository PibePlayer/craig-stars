using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    public class ResourcesTooltip : CSTooltip
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
                tipRichTextLabel.BbcodeText = $"[b]{Planet.Name}[/b] generates [b]{Planet.ResourcesPerYear}[/b] resources each year. " +
                $"[b]{Planet.ResourcesPerYearResearch}[/b] of these resources have been alloocated to research. That leaves " +
                $"[b]{Planet.ResourcesPerYearAvailable}[/b] resources for use by the planet";
            }
        }
    }
}