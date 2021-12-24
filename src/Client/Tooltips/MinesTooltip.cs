using System;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class MinesTooltip : CSTooltip
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


                if (Me.Race.Spec.InnateMining)
                {
                    tipRichTextLabel.BbcodeText += "Your race is incapable of building mines. However, your colonists have an innate mining ability equal to one tenth the square roote of the starbase population.";
                }
                else
                {
                    tipRichTextLabel.BbcodeText = $"You have [b]{Planet.Mines} Mines[/b] on [b]{Planet.Name}[/b]. " +
                    $"You may build up to [b]{Planet.Spec.MaxPossibleMines} Mines[/b]; however, your colonists are currently capable of operating only " +
                    $"[b]{Planet.Spec.MaxMines}[/b] of them.";
                }

            }
        }
    }
}