using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    public class PopulationTooltip : CSTooltip
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
                if (Planet.Player == Me)
                {
                    tipRichTextLabel.BbcodeText = $"Your population on [b]{Planet.Name}[/b] is [b]{Planet.Population:n0}[/b]." +
                        $"[b]{Planet.Name}[/b] will support a population of up to [b]{Planet.GetMaxPopulation(Me.Race, Me.Rules):n0}[/b]" +
                        $" of your colonists.\n" +
                        $"Your population on [b]{Planet.Name}[/b] will grow by {Planet.GrowthAmount:n0} to {Planet.Population + Planet.GrowthAmount:n0} next year.";
                }
                else if (Planet.Uninhabited && Planet.Explored)
                {
                    tipRichTextLabel.BbcodeText = $"[b]{Planet.Name}[/b] is uninhabited.\n" +
                    $"If you were to colonize [b]{Planet.Name}[/b], it would support up to [b]{Planet.GetMaxPopulation(Me.Race, Me.Rules):n0}[/b] " +
                    "of your colonists";
                }
                else if (!Planet.Uninhabited && Planet.Explored)
                {
                    tipRichTextLabel.BbcodeText = $"[b]{Planet.Name}[/b] is currently occupied by the [b]{Planet.Owner.RacePluralName}[/b].\n" +
                    $"If you were to colonize [b]{Planet.Name}[/b], it would support up to [b]{Planet.GetMaxPopulation(Me.Race, Me.Rules):n0}[/b] " +
                    "of your colonists";
                }
                else
                {
                    tipRichTextLabel.BbcodeText = $"[b]{Planet.Name}[/b] is unexplored.\n" +
                    $"Send a scout ship to this planet to determine its habitability.";
                }
            }
        }

    }
}