using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using Godot;
using log4net;
using static CraigStars.Utils.Utils;

namespace CraigStars
{
    /// <summary>
    /// Lay mines
    /// </summary>
    public class DecaySalvageStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(DecaySalvageStep));

        public DecaySalvageStep(Game game) : base(game, TurnGenerationState.DecaySalvage) { }

        public override void Process()
        {
            foreach (var salvage in Game.Salvage)
            {
                Decay(salvage);
            }
        }

        /// <summary>
        /// https://wiki.starsautohost.org/wiki/Guts_of_scrapping
        /// In deep space, each type of mineral decays 10%, or 10kT per year, whichever is higher. Salvage deposited on planets does not decay.
        /// </summary>
        /// <param name="salvage"></param>
        internal void Decay(Salvage salvage)
        {
            // decay salvage by 10kt or 10%, whichever is greater
            salvage.Cargo = new Cargo(
                (int)Math.Max(0, Math.Min(
                    salvage.Cargo.Ironium - salvage.Cargo.Ironium * Game.Rules.SalvageDecayRate,
                    salvage.Cargo.Ironium - Game.Rules.SalvageDecayMin
                )),
                (int)Math.Max(0, Math.Min(
                    salvage.Cargo.Boranium - salvage.Cargo.Boranium * Game.Rules.SalvageDecayRate,
                    salvage.Cargo.Boranium - Game.Rules.SalvageDecayMin
                )),
                (int)Math.Max(0, Math.Min(
                    salvage.Cargo.Germanium - salvage.Cargo.Germanium * Game.Rules.SalvageDecayRate,
                    salvage.Cargo.Germanium - Game.Rules.SalvageDecayMin
                ))
            );

            // remove empty salvage
            if (salvage.Cargo == Cargo.Empty)
            {
                EventManager.PublishMapObjectDeletedEvent(salvage);
            }

        }


    }
}