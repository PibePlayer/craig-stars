using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    public class FactoriesTooltip : CSTooltip
    {
        PlanetService planetService = new();
        RichTextLabel tipRichTextLabel;

        public override void _Ready()
        {
            tipRichTextLabel = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/TipRichTextLabel");
        }

        protected override void UpdateControls()
        {
            if (Planet != null)
            {
                tipRichTextLabel.BbcodeText = $"You have [b]{Planet.Factories} Factories[/b] on [b]{Planet.Name}[/b]. " +
                $"You may build up to [b]{planetService.GetMaxPossibleFactories(Planet, Me)} Factories[/b]; however, your colonists are currently capable of operating only " +
                $"[b]{planetService.GetMaxFactories(Planet, Me)}[/b] of them";
            }
        }
    }
}