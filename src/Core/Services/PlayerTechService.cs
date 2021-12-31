using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static CraigStars.Utils.Utils;

namespace CraigStars
{
    public class PlayerTechService
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlayerTechService));

        private readonly IProvider<ITechStore> techStoreProvider;

        private ITechStore TechStore { get => techStoreProvider.Item; }

        public PlayerTechService(IProvider<ITechStore> techStoreProvider)
        {
            this.techStoreProvider = techStoreProvider;
        }

        /// <summary>
        /// Get a list of all techs available to the player
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public IEnumerable<Tech> GetAvailableTechs(Player player)
        {
            return TechStore.Techs.Where(tech => HasTech(player, tech)).ToList();
        }

        /// <summary>
        /// Returns true if the player has this tech
        /// </summary>
        /// <param name="tech">The tech to check requirements for</param>
        /// <returns>True if this player has access to this tech</returns>
        public bool HasTech(Player player, Tech tech)
        {
            // we made it here, if we have the levels, we have the tech
            return CanLearnTech(player, tech) && player.TechLevels.HasRequiredLevels(tech.Requirements);
        }

        /// <summary>
        /// Can the player ever learn this tech?
        /// </summary>
        /// <param name="tech"></param>
        /// <returns></returns>
        public bool CanLearnTech(Player player, Tech tech)
        {
            TechRequirements requirements = tech.Requirements;
            if (requirements.PRTRequired != PRT.None && requirements.PRTRequired != player.Race.PRT)
            {
                return false;
            }
            if (requirements.PRTDenied != PRT.None && player.Race.PRT == requirements.PRTDenied)
            {
                return false;
            }

            foreach (LRT lrt in requirements.LRTsRequired)
            {
                if (!player.Race.HasLRT(lrt))
                {
                    return false;
                }
            }

            foreach (LRT lrt in requirements.LRTsDenied)
            {
                if (player.Race.HasLRT(lrt))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get the best planetary scanner this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHull GetBestOrbitalConstructionHull(Player player)
        {
            var techs = TechStore.GetTechsByCategory(TechCategory.StarbaseHull)
                .Where(t => t is TechHull th && th.OrbitalConstructionHull)
                .Where(t => HasTech(player, t))
                .OrderBy(t => t.Ranking)
                .ToList();

            return techs.FirstOrDefault() as TechHull;
        }

        /// <summary>
        /// Get the best planetary scanner this player has access to
        /// </summary>
        /// <returns></returns>
        public TechPlanetaryScanner GetBestPlanetaryScanner(Player player)
        {
            return GetBestTech<TechPlanetaryScanner>(player, TechCategory.PlanetaryScanner);
        }

        /// <summary>
        /// Get the best beam weapon this player has access to
        /// </summary>
        /// <returns></returns>
        public TechDefense GetBestDefense(Player player)
        {
            return GetBestTech<TechDefense>(player, TechCategory.PlanetaryDefense);
        }

        /// <summary>
        /// Get the best terraform tech this player has for a terraform type
        /// </summary>
        /// <returns></returns>
        public TechTerraform GetBestTerraform(Player player, TerraformHabType habType)
        {
            var techs = TechStore.GetTechsByCategory(TechCategory.Terraforming).Where(t => t is TechTerraform tf && tf.HabType == habType).ToList();

            techs.Sort((t1, t2) => t2.Ranking.CompareTo(t1.Ranking));
            var tech = techs.Find(t => HasTech(player, t));

            return tech as TechTerraform;
        }

        /// <summary>
        /// Get the best engine this player has access to
        /// </summary>
        /// <returns></returns>
        public TechEngine GetBestEngine(Player player, bool colonistTransport = false)
        {
            var techs = TechStore.GetTechsByCategory(TechCategory.Engine);
            return techs
                .Where(t => HasTech(player, t))
                .Cast<TechEngine>()
                // if this is a colonist transport and we are damaged by radiation, don't include radiating engines
                .Where(engine => !colonistTransport || !(engine.Radiating && player.Race.IsDamagedByRadiation))
                .OrderByDescending(t => t.Ranking)
                .FirstOrDefault();
        }

        /// <summary>
        /// Get the best shield this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestScanner(Player player)
        {
            return GetBestTech<TechHullComponent>(player, TechCategory.Scanner);
        }

        /// <summary>
        /// Get the best shield this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestShield(Player player)
        {
            return GetBestTech<TechHullComponent>(player, TechCategory.Shield);
        }

        /// <summary>
        /// Get the best armor this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestArmor(Player player)
        {
            return GetBestTech<TechHullComponent>(player, TechCategory.Armor);
        }

        /// <summary>
        /// Get the best beam weapon this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechHullComponent GetBestBeamWeapon(Player player)
        {
            return GetBestTech<TechHullComponent>(player, TechCategory.BeamWeapon);
        }

        /// <summary>
        /// Get the best torpedo this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechHullComponent GetBestTorpedo(Player player)
        {
            return GetBestTech<TechHullComponent>(player, TechCategory.Torpedo);
        }

        /// <summary>
        /// Get the best bomb this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestBomb(Player player)
        {
            return GetBestTech<TechHullComponent>(player, TechCategory.Bomb);
        }

        /// <summary>
        /// Get the best mine robot this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechHullComponent GetBestMineRobot(Player player)
        {
            var techs = TechStore.GetTechsByCategory(TechCategory.MineRobot)
                .Where(t => t is TechHullComponent hc && HasTech(player, hc) && hc.MiningRate != 0)
                .Cast<TechHullComponent>()
                .OrderByDescending(t => t.MiningRate);


            return techs.FirstOrDefault() as TechHullComponent;
        }

        /// <summary>
        /// Get the best mine layer this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestTerraformer(Player player)
        {
            var techs = TechStore.GetTechsByCategory(TechCategory.MineRobot)
                .Where(t => t is TechHullComponent hc && HasTech(player, hc) && hc.TerraformRate > 0)
                .OrderByDescending(t => t.Ranking);


            return techs.FirstOrDefault() as TechHullComponent;
        }

        /// <summary>
        /// Get the best fuel tank this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestFuelTank(Player player)
        {
            var techs = TechStore.GetTechsByCategory(TechCategory.Mechanical)
                .Where(t => t is TechHullComponent hc && HasTech(player, hc) && hc.FuelBonus > 0)
                .OrderByDescending(t => t is TechHullComponent hc ? hc.FuelBonus : 0);

            return techs.FirstOrDefault() as TechHullComponent;
        }

        /// <summary>
        /// Get the best cargo pod this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestCargoPod(Player player)
        {
            var techs = TechStore.GetTechsByCategory(TechCategory.Mechanical)
                .Where(t => t is TechHullComponent hc && HasTech(player, hc) && hc.CargoBonus > 0)
                .OrderByDescending(t => t is TechHullComponent hc ? hc.FuelBonus : 0);

            return techs.FirstOrDefault() as TechHullComponent;
        }

        /// <summary>
        /// Get the best mine layer this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestMineLayer(Player player)
        {
            var techs = TechStore.GetTechsByCategory(TechCategory.MineLayer)
                .Where(t => t is TechHullComponent hc && HasTech(player, hc) && hc.MineFieldType != MineFieldType.SpeedBump)
                .OrderByDescending(t => t.Ranking);

            return techs.FirstOrDefault() as TechHullComponent;
        }

        /// <summary>
        /// Get the best mine layer this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestSpeedTrapLayer(Player player)
        {
            var techs = TechStore.GetTechsByCategory(TechCategory.MineLayer)
                .Where(t => t is TechHullComponent hc && HasTech(player, hc) && hc.MineFieldType == MineFieldType.SpeedBump)
                .OrderByDescending(t => t.Ranking);


            return techs.FirstOrDefault() as TechHullComponent;
        }

        /// <summary>
        /// Get the best mine layer this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestStargate(Player player)
        {
            var techs = TechStore.GetTechsByCategory(TechCategory.Orbital)
                .Where(t => t is TechHullComponent hc && HasTech(player, hc) && hc.SafeRange > 0)
                .OrderByDescending(t => t.Ranking);

            return techs.FirstOrDefault() as TechHullComponent;
        }

        /// <summary>
        /// Get the best mine layer this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestMassDriver(Player player)
        {

            var techs = TechStore.GetTechsByCategory(TechCategory.Orbital)
                .Where(t => t is TechHullComponent hc && HasTech(player, hc) && hc.PacketSpeed > 0)
                .OrderByDescending(t => t.Ranking);

            return techs.FirstOrDefault() as TechHullComponent;
        }

        /// <summary>
        /// Get the best mine layer this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestColonizationModule(Player player)
        {
            IEnumerable<Tech> techs;
            if (player.Race.Spec.LivesOnStarbases)
            {
                techs = TechStore.GetTechsByCategory(TechCategory.Mechanical)
                    .Where(t => t is TechHullComponent hc && HasTech(player, hc) && hc.OrbitalConstructionModule)
                    .OrderByDescending(t => t.Ranking);
            }
            else
            {
                techs = TechStore.GetTechsByCategory(TechCategory.Mechanical)
                    .Where(t => t is TechHullComponent hc && HasTech(player, hc) && hc.ColonizationModule)
                    .OrderByDescending(t => t.Ranking);
            }

            return techs.FirstOrDefault() as TechHullComponent;
        }

        /// <summary>
        /// Get the best tech by category
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="category"></param>
        /// <returns></returns>
        public T GetBestTech<T>(Player player, TechCategory category) where T : Tech
        {
            var techs = TechStore.GetTechsByCategory(category);
            var tech = techs
                .Where(t => HasTech(player, t))
                .OrderByDescending(t => t.Ranking).FirstOrDefault();
            return tech as T;
        }
    }
}