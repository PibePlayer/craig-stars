using System;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class RadiationTooltip : CSTooltip
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
                var type = HabType.Radiation;
                var typeName = "Radiation";
                var currentHab = Planet.Hab.Value[type];
                var currentHabString = TextUtils.GetRadString(currentHab);
                var habLowString = TextUtils.GetRadString(Me.Race.HabLow.rad);
                var habHighString = TextUtils.GetRadString(Me.Race.HabHigh.rad);
                var text = @$"
{typeName} is currently [b]{currentHabString}[/b].  
Your colonists prefer planets where {typeName} is between [b]{habLowString}[/b] 
and [b]{habHighString}[/b].".Replace("\n", "");

                // if we can terraform, show it
                Hab terraformAmount = Planet.Spec.TerraformAmount;
                if (terraformAmount[type] != 0)
                {
                    var terraformedHab = Planet.Hab.Value.WithRad(Planet.Hab.Value[type] + terraformAmount[type]);
                    var terraformedHabString = TextUtils.GetRadString(terraformedHab[type]);
                    text += @$"
  You currently possess the technology to modify the {typeName} on [b]{Planet.Name}[/b] within the range of 
within the range of [b]{(currentHab <= terraformedHab[type] ? currentHabString : terraformedHabString)}[/b] to 
[b]{(currentHab > terraformedHab[type] ? currentHabString : terraformedHabString)}[/b].  
If you were to terraform [b]{typeName}[/b] to [b]{terraformedHabString}[/b], the planet's value would improve 
to [b]{Me.Race.GetPlanetHabitability(terraformedHab)}%[/b].".Replace("\n", "");
                    // todo: add "This value is 12% away from the ideal value for your race"
                }

                tipRichTextLabel.BbcodeText = text;
            }
        }
    }
}