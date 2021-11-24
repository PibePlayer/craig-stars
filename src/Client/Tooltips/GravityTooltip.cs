using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System;

namespace CraigStars.Client
{
    public class GravityTooltip : CSTooltip
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
                var type = HabType.Gravity;
                var typeName = "Gravity";
                var currentHab = Planet.Hab.Value[type];
                var currentHabString = TextUtils.GetGravString(currentHab);
                var habLowString = TextUtils.GetGravString(Me.Race.HabLow.grav);
                var habHighString = TextUtils.GetGravString(Me.Race.HabHigh.grav);
                var text = @$"
{typeName} is currently [b]{currentHabString}[/b].  
Your colonists prefer planets where {typeName} is between [b]{habLowString}[/b] 
and [b]{habHighString}[/b].".Replace("\n", "");

                // if we can terraform, show it
                Hab terraformAmount = Planet.Spec.TerraformAmount;
                if (terraformAmount[type] != 0)
                {
                    var terraformedHab = Planet.Hab.Value.WithGrav(Planet.Hab.Value[type] + terraformAmount[type]);
                    var terraformedHabString = TextUtils.GetGravString(terraformedHab[type]);
                    text += @$"
  You currently possess the technology to modify the Gravity on [b]{Planet.Name}[/b] within the range of 
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