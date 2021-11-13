using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars.Client
{
    public class FutureTechs : MarginContainer
    {
        [Inject] protected PlayerTechService playerTechService;

        protected Player Me { get => PlayersManager.Me; }

        Container techsContainer;
        public override void _Ready()
        {
            this.ResolveDependencies();
            techsContainer = GetNode<Container>("ScrollContainer/TechsContainer");
        }

        /// <summary>
        /// Update the techs that will be researched with this current plan
        /// </summary>
        public void UpdateIncomingTechs(TechField field)
        {
            var currentLevel = Me.TechLevels[field];

            var futureTechs = new List<FutureTech>();

            // for all techs we don't have (but can get), see how far away we will get it with some future level
            foreach (var tech in TechStore.Instance.Techs.Where(t => !playerTechService.HasTech(Me, t) && playerTechService.CanLearnTech(Me, t)))
            {
                var distanceToLearn = tech.Requirements - Me.TechLevels;
                // zero out any level differences we have already achieved
                // i.e. if we are at level 5 for energy and this tech requires 3, distanceToLearn.Energy will equal -2
                // this makes it zero
                distanceToLearn = new TechLevel(
                    Math.Max(0, distanceToLearn.Energy),
                    Math.Max(0, distanceToLearn.Weapons),
                    Math.Max(0, distanceToLearn.Propulsion),
                    Math.Max(0, distanceToLearn.Construction),
                    Math.Max(0, distanceToLearn.Electronics),
                    Math.Max(0, distanceToLearn.Biotechnology)
                );

                if (distanceToLearn.Sum() == distanceToLearn[field])
                {
                    // if the required tech difference is only in the field we care about
                    // add it to our list of future techs
                    futureTechs.Add(new FutureTech() { Tech = tech, Distance = distanceToLearn[field] });
                }
            }

            foreach (Node child in techsContainer.GetChildren())
            {
                child.QueueFree();
            }

            foreach (var futureTech in futureTechs.OrderBy(ft => ft.Distance))
            {
                var color = Colors.White;
                if (futureTech.Distance > 1 && futureTech.Distance < 5)
                {
                    color = GUIColorsProvider.Colors.ProductionQueueMoreThanOneYearColor;
                }
                else if (futureTech.Distance <= 1)
                {
                    color = GUIColorsProvider.Colors.ProductionQueueItemOneYearColor;
                }

                techsContainer.AddChild(new TechLabel()
                {
                    Tech = futureTech.Tech,
                    Text = $"{futureTech.Tech.Name}",
                    Modulate = color
                });
            }
        }
    }
}