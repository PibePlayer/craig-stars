using System;
using System.Collections.Generic;
using log4net;

using CraigStars.Utils;

namespace CraigStars
{
    /// <summary>
    /// Class to generate designs for players
    /// </summary>
    public class ShipDesignGenerator
    {
        static CSLog log = LogProvider.GetLogger(typeof(ShipDesignGenerator));

        private readonly PlayerTechService playerTechService;

        public ShipDesignGenerator(PlayerTechService playerTechService)
        {
            this.playerTechService = playerTechService;
        }

        public delegate TechHullComponent FillGeneralSlotCallback(int index);

        public ShipDesign DesignShip(TechHull hull, string name, Player player, int hullSetNumber, ShipDesignPurpose purpose)
        {
            var design = new ShipDesign()
            {
                Name = name,
                PlayerNum = player.Num,
                Hull = hull,
                HullSetNumber = hullSetNumber,
                Purpose = purpose
            };

            // make an even number of beam and torpedo slots
            int torpedoSlots = 0;
            int beamSlots = 0;

            int numShields = 0;
            int numArmor = 0;
            int numScanners = 0;
            int numFuelTanks = 0;
            int numCargoPods = 0;


            var bestStargate = playerTechService.GetBestStargate(player);
            var bestMassDriver = playerTechService.GetBestMassDriver(player);
            var bestFuelTank = playerTechService.GetBestFuelTank(player);
            var bestCargoPod = playerTechService.GetBestCargoPod(player);

            // if we have no stargate available, that's the same as already assigning one
            bool stargate = !(bestStargate != null);
            bool massDriver = !(bestMassDriver != null);

            // populate each slot for this design
            hull.Slots.Each((hullSlot, index) =>
            {
                ShipDesignSlot slot = new ShipDesignSlot()
                {
                    HullSlotIndex = index + 1
                };
                // fill er up!
                slot.Quantity = hullSlot.Capacity;

                switch (hullSlot.Type)
                {
                    case HullSlotType.Engine:
                        slot.HullComponent = playerTechService.GetBestEngine(player);
                        break;
                    case HullSlotType.Electrical:
                        // TODO: use GetBestBattleComputer()/GetBestJammer()
                        slot.HullComponent = Techs.BattleComputer;
                        break;
                    case HullSlotType.Scanner:
                        slot.HullComponent = playerTechService.GetBestScanner(player);
                        numScanners++;
                        break;
                    case HullSlotType.Mechanical:
                    case HullSlotType.ScannerElectricalMechanical:
                        switch (purpose)
                        {
                            case ShipDesignPurpose.Colonizer:
                                slot.HullComponent = playerTechService.GetBestColonizationModule(player);
                                slot.Quantity = 1; // we only need 1 colonization module
                                break;
                            default:
                                if (hullSlot.Type.HasFlag(HullSlotType.Scanner) && numScanners == 0)
                                {
                                    slot.HullComponent = playerTechService.GetBestScanner(player);
                                    numScanners++;
                                }
                                else if (numFuelTanks == 0 || hull.CargoCapacity == 0)
                                {
                                    slot.HullComponent = bestFuelTank;
                                    numFuelTanks++;
                                }
                                else
                                {
                                    slot.HullComponent = bestCargoPod;
                                    numCargoPods++;
                                }
                                break;

                        }
                        break;
                    case HullSlotType.Shield:
                        slot.HullComponent = playerTechService.GetBestShield(player);
                        numShields++;
                        break;
                    case HullSlotType.Armor:
                        numArmor++;
                        slot.HullComponent = playerTechService.GetBestArmor(player);
                        break;
                    case HullSlotType.ShieldArmor:
                        // balance armor and shields, but if equal, do armor
                        // armor is heavy though, so don't use it on freighters.
                        if (numShields >= numArmor && purpose != ShipDesignPurpose.Colonizer && purpose != ShipDesignPurpose.Freighter && purpose != ShipDesignPurpose.FuelFreighter)
                        {
                            slot.HullComponent = playerTechService.GetBestArmor(player);
                        }
                        else
                        {
                            slot.HullComponent = playerTechService.GetBestShield(player);
                        }
                        break;
                    case HullSlotType.Weapon:
                        // balance beams and torpedos, but if equal, do beams
                        if (torpedoSlots >= beamSlots)
                        {
                            slot.HullComponent = playerTechService.GetBestBeamWeapon(player);
                            beamSlots++;
                        }
                        else
                        {
                            slot.HullComponent = playerTechService.GetBestTorpedo(player);
                            torpedoSlots++;
                        }
                        break;
                    case HullSlotType.Mining:
                        slot.HullComponent = playerTechService.GetBestMineRobot(player);
                        break;
                    case HullSlotType.MineLayer:
                        if (design.Purpose == ShipDesignPurpose.SpeedMineLayer)
                        {
                            slot.HullComponent = playerTechService.GetBestMineLayer(player);
                        }
                        else
                        {
                            slot.HullComponent = playerTechService.GetBestSpeedTrapLayer(player);
                        }
                        break;
                    case HullSlotType.Orbital:
                    case HullSlotType.OrbitalElectrical:
                        if (!stargate)
                        {
                            slot.HullComponent = bestStargate;
                            stargate = true;
                        }
                        else if (!massDriver)
                        {
                            slot.HullComponent = bestMassDriver;
                            massDriver = true;
                        }
                        else if (hullSlot.Type == HullSlotType.OrbitalElectrical)
                        {
                            // TODO: write GetBestJammer()
                            // slot.HullComponent = playerTechService.GetBestJammer(player);
                        }
                        break;
                    case HullSlotType.Bomb:
                        slot.HullComponent = playerTechService.GetBestBomb(player);
                        break;
                    case HullSlotType.General:
                        switch (purpose)
                        {
                            case ShipDesignPurpose.ArmedScout:
                                slot.HullComponent = playerTechService.GetBestBeamWeapon(player);
                                break;
                            case ShipDesignPurpose.Freighter:
                                slot.HullComponent = bestCargoPod;
                                numCargoPods++;
                                break;
                            case ShipDesignPurpose.FuelFreighter:
                                slot.HullComponent = bestFuelTank;
                                numFuelTanks++;
                                break;
                            case ShipDesignPurpose.FighterScout:
                                slot.HullComponent = playerTechService.GetBestScanner(player);
                                break;
                            case ShipDesignPurpose.DamageMineLayer:
                                slot.HullComponent = playerTechService.GetBestMineLayer(player);
                                break;
                            default:
                                slot.HullComponent = bestFuelTank;
                                numFuelTanks++;
                                break;
                        }
                        break;
                }
                design.Slots.Add(slot);
            });

            design.ComputeAggregate(player);
            return design;
        }
    }
}