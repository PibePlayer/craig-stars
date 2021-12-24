using System;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class FactoriesTooltip : CSTooltip
    {
        [Inject] protected PlanetService planetService;

        RichTextLabel tipRichTextLabel;

        public override void _Ready()
        {
            this.ResolveDependencies();
            tipRichTextLabel = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/TipRichTextLabel");
        }

        protected override void UpdateControls()
        {
            if (Planet != null)
            {
                if (Me.Race.Spec.InnateResources)
                {
                    tipRichTextLabel.BbcodeText = $"Your race is incapable of building factories.";
                }
                else
                {
                    tipRichTextLabel.BbcodeText = $"You have [b]{Planet.Factories} Factories[/b] on [b]{Planet.Name}[/b]. " +
                    $"You may build up to [b]{Planet.Spec.MaxPossibleFactories} Factories[/b]; however, your colonists are currently capable of operating only " +
                    $"[b]{Planet.Spec.MaxFactories}[/b] of them";
                }
            }
        }
    }
}