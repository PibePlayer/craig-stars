using System;
using System.Collections.Generic;

namespace CraigStars.Utils
{
    public static class EnumUtils
    {

        public static string GetLabelForWaypointTask(WaypointTask task)
        {
            switch (task)
            {
                case WaypointTask.RemoteMining:
                    return "Remote Mining";
                case WaypointTask.MergeWithFleet:
                    return "Merge With Fleet";
                case WaypointTask.ScrapFleet:
                    return "Scrap Fleet";
                case WaypointTask.LayMineField:
                    return "Lay Mine Field";
                case WaypointTask.TransferFleet:
                    return "Transfer";
                default:
                    return task.ToString();
            }
        }

        public static string GetLabelForSpendLeftoverPointsOn(SpendLeftoverPointsOn value)
        {
            switch (value)
            {
                case SpendLeftoverPointsOn.MineralConcentrations:
                    return "Mineral Concentrations";
                case SpendLeftoverPointsOn.SurfaceMinerals:
                    return "Surface Minerals";
                default:
                    return value.ToString();
            }

        }

        public static string GetLabelForLRT(LRT lrt)
        {
            switch (lrt)
            {
                case LRT.IFE:
                    return "Improved Fuel Efficiency";
                case LRT.TT:
                    return "Total Terraforming";
                case LRT.ARM:
                    return "Advanced Remote Mining";
                case LRT.ISB:
                    return "Improved Starbases";
                case LRT.GR:
                    return "Generalized Research";
                case LRT.UR:
                    return "Ultimate Recycling";
                case LRT.NRSE:
                    return "No Ram Scoop Engines";
                case LRT.OBRM:
                    return "Only Basic Remote Mining";
                case LRT.NAS:
                    return "No Advanced Scanners";
                case LRT.LSP:
                    return "Low Starting Population";
                case LRT.BET:
                    return "Bleeding Edge Technology";
                case LRT.RS:
                    return "Regenerating Shields";
                case LRT.MA:
                    return "Mineral Alchemy";
                case LRT.CE:
                    return "Cheap Engines";
                default:
                    return lrt.ToString();
            }
        }
    }
}

