using System;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
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
                tipRichTextLabel.BbcodeText = $"[b]{Planet.Name}[/b] generates [b]{Planet.Spec.ResourcesPerYear}[/b] resources each year. " +
                $"[b]{Planet.Spec.ResourcesPerYearResearch}[/b] of these resources have been alloocated to research. That leaves " +
                $"[b]{Planet.Spec.ResourcesPerYearAvailable}[/b] resources for use by the planet.";

                if (Me.Race.Spec.InnateResources)
                {
                    tipRichTextLabel.BbcodeText += "  Your resources at the planet are increase with population and [b]Energy[/b] tech.";
                }
            }
        }
    }
}