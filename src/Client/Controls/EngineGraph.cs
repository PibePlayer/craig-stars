using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars.Client
{

    public class EngineGraph : Graph
    {
        static CSLog log = LogProvider.GetLogger(typeof(EngineGraph));

        public TechEngine Engine { get; set; } = Techs.QuickJump5;

        const int MaxFuelUsage = 800;

        public EngineGraph()
        {
            XAxisLabels = Enumerable.Range(0, 11).Select(x => new AxisLabel(x.ToString(), x, x == Engine.IdealSpeed ? Colors.LightBlue : Colors.White)).ToList();

            YAxisLabels.Add(new AxisLabel("0%", 0, Colors.White));
            for (float yAxisValue = .25f; yAxisValue <= (MaxFuelUsage / 100); yAxisValue *= 2)
            {
                // percent used for this index
                var percent = (yAxisValue) * 100;

                YAxisLabels.Add(new AxisLabel($"{percent}%", percent, percent > 100 ? Colors.Red : Colors.White));
            }
        }

        public override void _Ready()
        {
            base._Ready();
        }

        protected override List<Vector2> GetPoints()
        {
            List<Vector2> points = new();

            for (int warp = 0; warp < Engine.FuelUsage.Length; warp++)
            {
                float usage = Engine.FuelUsage[warp];

                float scaledUsage = 0;
                if (usage > 0)
                {
                    // scale the usage in powers of 25, i.e. usage of 25 = 1, usage of 50 = 2, usage of 100 = 3, etc
                    // this is because our indexes on the y axis are 25, 50, 100, 200, 400, 800 which is like
                    // usageIndex = 25 * 2^(y - 1), solve for y and you get this nastiness below
                    scaledUsage = Math.Max(0, (float)((Mathf.Log(usage / 25)) / Mathf.Log(2)) + 1);
                }

                log.Info($"Warp: {warp}, Usage: {usage}, Scaled Usage: {scaledUsage}");
                points.Add(new Vector2(warp, scaledUsage));
            }

            return points;
        }

    }
}