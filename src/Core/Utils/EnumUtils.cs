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

    }
}

