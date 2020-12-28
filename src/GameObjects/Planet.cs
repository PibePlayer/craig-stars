using System.Collections.Generic;
using System.Linq;
using Godot;
using CraigStars.Singletons;

namespace CraigStars
{
    public class Planet : MapObject
    {
        public enum Orbiting
        {
            None,
            Orbiting,
            OrbitingEnemies,
            OrbitingAlliesAndEnemies
        }

        [Export]
        public Orbiting OrbitingState { get; set; } = Orbiting.None;

        PlanetSprite sprite;

        #region Planet Stats

        public Hab Hab { get; set; } = new Hab();
        public Mineral MineralConcentration { get; set; } = new Mineral();

        public Mineral MineYears { get; set; } = new Mineral();
        public Cargo Cargo { get; } = new Cargo();

        public int Population { get => Cargo.Population; set { Cargo.Population = value; } }
        public int PopulationDensity { get => Population > 0 ? Population / GetMaxPopulation() : 0; }

        public Player Player { get; set; }
        public int Mines { get; set; }
        public int MaxMines { get => (Population > 0 && Player != null) ? Population / 10000 * Player.Race.NumMines : 0; }
        public int Factories { get; set; }
        public int MaxFactories { get => (Population > 0 && Player != null) ? Population / 10000 * Player.Race.NumFactories : 0; }

        public int Defenses { get; set; }
        public int MaxDefenses { get => (Population > 0 && Player != null) ? Population / 10000 * 10 : 0; }
        public bool ContributesToResearch { get; set; }
        public bool Homeworld { get; set; }
        public bool Scanner { get; set; }

        public int ReportAge { get; set; } = 0; // = -1; // -1 means unexplored
        public bool Explored { get => ReportAge != -1; }

        public List<Fleet> OrbitingFleets { get; set; } = new List<Fleet>();
        #endregion

        public override void _Ready()
        {
            base._Ready();
            sprite = GetNode<PlanetSprite>("Sprite");
        }

        protected override void OnSelected()
        {
            base.OnSelected();
            sprite.UpdateVisibleSprites(Player, this);
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            sprite.UpdateVisibleSprites(Player, this);
        }

        protected override void OnDeselected()
        {
            base.OnDeselected();
            sprite.UpdateVisibleSprites(Player, this);
        }

        internal override List<MapObject> GetPeers()
        {
            return OrbitingFleets.Where(f => f.Player == Player).ToList<MapObject>();
        }

        public void UpdateVisibleSprites()
        {
            sprite.UpdateVisibleSprites(Player, this);
        }

        /// <summary>
        /// The max population for the race on this planet
        /// TODO: support this later
        /// /// </summary>
        /// <returns></returns>
        public int GetMaxPopulation()
        {
            return 1000000;
        }

        public void Grow()
        {
            Population += GetGrowthAmount();
        }

        /// <summary>
        /// The amount the population for this planet will grow next turn
        /// </summary>
        /// <returns></returns>
        public int GetGrowthAmount()
        {
            var race = Player?.Race;
            if (race != null)
            {
                double capacity = (double)(Population / GetMaxPopulation());
                int popGrowth = (int)((double)(Population) * (race.GrowthRate / 100.0) * ((double)(race.GetPlanetHabitability(Hab)) / 100.0));

                if (capacity > .25)
                {
                    double crowdingFactor = 16.0 / 9.0 * (1.0 - capacity) * (1.0 - capacity);
                    popGrowth = (int)((double)(popGrowth) * crowdingFactor);
                }

                return popGrowth;

            }
            return 0;
        }

        /// <summary>
        /// The mineral output of this planet if it is owned
        /// </summary>
        /// <returns></returns>
        public Mineral GetMineralOutput()
        {
            var race = Player?.Race;
            if (race != null)
            {
                var mineOutput = race.MineOutput;
                return new Mineral(
                    MineralsPerYear(MineralConcentration.Ironium, Mines, mineOutput),
                    MineralsPerYear(MineralConcentration.Boranium, Mines, mineOutput),
                    MineralsPerYear(MineralConcentration.Germanium, Mines, mineOutput)
                );
            }
            return Mineral.Empty;
        }

        /// <summary>
        /// Get the amount of minerals mined in one year, for one type
        /// </summary>
        /// <param name="mineralConcentration">The concentration of minerals</param>
        /// <param name="mines">The number of mines on the planet</param>
        /// <param name="mineOutput">The mine output for the owner race</param>
        /// <returns>The mineral output for one year for one mineral conc</returns>
        int MineralsPerYear(int mineralConcentration, int mines, int mineOutput)
        {
            return (int)(((float)(mineralConcentration) / 100.0) * ((float)(mines) / 10.0) * (float)(mineOutput));
        }
    }
}
