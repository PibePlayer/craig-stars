using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    public class PopulationTooltip : CSTooltip
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
                if (Planet.OwnedBy(Me))
                {
                    var hab = Me.Race.GetPlanetHabitability(Planet.Hab.Value);
                    var growthAmount = planetService.GetGrowthAmount(Planet, Me, RulesManager.Rules);
                    if (hab > 0)
                    {
                        tipRichTextLabel.BbcodeText = $"Your population on [b]{Planet.Name}[/b] is [b]{Planet.Population:n0}[/b]." +
                            $"[b]{Planet.Name}[/b] will support a population of up to [b]{planetService.GetMaxPopulation(Planet, Me, RulesManager.Rules):n0}[/b]" +
                            $" of your colonists.\n";
                    }
                    else
                    {
                        tipRichTextLabel.BbcodeText = $"Your population on [b]{Planet.Name}[/b] is [b]{Planet.Population:n0}[/b]." +
                            $"[b]{Planet.Name}[/b] has a hostile environment and will no support any of your colonists.\n";
                    }
                    if (growthAmount > 0)
                    {
                        tipRichTextLabel.BbcodeText += $"Your population on [b]{Planet.Name}[/b] will grow by {growthAmount:n0} to {Planet.Population + growthAmount:n0} next year.";
                    }
                    else if (growthAmount == 0)
                    {
                        tipRichTextLabel.BbcodeText += $"Your population on [b]{Planet.Name}[/b] will not grow next year.";
                    }
                    else if (growthAmount < 0)
                    {
                        tipRichTextLabel.BbcodeText += $"Approximately {Math.Abs(growthAmount):n0} of your colonists will die next year.";
                    }
                }
                else if (!Planet.Owned && Planet.Explored)
                {
                    var hab = Me.Race.GetPlanetHabitability(Planet.Hab.Value);
                    tipRichTextLabel.BbcodeText = $"[b]{Planet.Name}[/b] is uninhabited.\n";
                    if (hab > 0)
                    {
                        tipRichTextLabel.BbcodeText += $"If you were to colonize [b]{Planet.Name}[/b], it would support up to [b]{planetService.GetMaxPopulation(Planet, Me, RulesManager.Rules):n0}[/b] " +
                            "of your colonists";
                    }
                    else
                    {
                        tipRichTextLabel.BbcodeText += $"[b]{Planet.Name}[/b] will kill off approximately [b]{Math.Abs(hab) / 10f:.#}%[/b] of all colonists you settle on it every turn.";
                    }
                }
                else if (Planet.Owned && Planet.Explored)
                {
                    tipRichTextLabel.BbcodeText = $"[b]{Planet.Name}[/b] is currently occupied by the [b]{Planet.RacePluralName}[/b].\n";

                    var hab = Me.Race.GetPlanetHabitability(Planet.Hab.Value);
                    tipRichTextLabel.BbcodeText = $"[b]{Planet.Name}[/b] is uninhabited.\n";
                    if (hab > 0)
                    {
                        tipRichTextLabel.BbcodeText += $"If you were to colonize [b]{Planet.Name}[/b], it would support up to [b]{planetService.GetMaxPopulation(Planet, Me, RulesManager.Rules):n0}[/b] " +
                            "of your colonists";
                    }
                    else
                    {
                        tipRichTextLabel.BbcodeText += $"[b]{Planet.Name}[/b] will kill off approximately [b]{Math.Abs(hab) / 10f:.#}%[ /b] of all colonists you settle on it every turn.";
                    }

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