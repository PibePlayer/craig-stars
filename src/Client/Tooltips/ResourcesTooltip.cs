using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    public class ResourcesTooltip : CSTooltip
    {
        PlanetService planetService = new();
        RichTextLabel tipRichTextLabel;

        public override void _Ready()
        {
            tipRichTextLabel = GetNode<RichTextLabel>("MarginContainer/TipRichTextLabel");
        }

        protected override void UpdateControls()
        {
            if (Planet != null)
            {
                tipRichTextLabel.BbcodeText = $"[b]{Planet.Name}[/b] generates [b]{planetService.GetResourcesPerYear(Planet, Me)}[/b] resources each year. " +
                $"[b]{planetService.GetResourcesPerYearResearch(Planet, Me)}[/b] of these resources have been alloocated to research. That leaves " +
                $"[b]{planetService.GetResourcesPerYearAvailable(Planet, Me)}[/b] resources for use by the planet";
            }
        }
    }
}