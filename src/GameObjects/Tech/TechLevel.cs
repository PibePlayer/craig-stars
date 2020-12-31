namespace CraigStars
{
    public class TechLevel
    {
        public int Energy { get; set; }
        public int Weapons { get; set; }
        public int Propulsion { get; set; }
        public int Construction { get; set; }
        public int Electronics { get; set; }
        public int Biotechnology { get; set; }

        public TechLevel() { }
        public TechLevel(int energy = 0, int weapons = 0, int propulsion = 0, int construction = 0, int electronics = 0, int biotechnology = 0)
        {
            Energy = energy;
            Weapons = weapons;
            Propulsion = propulsion;
            Construction = construction;
            Electronics = electronics;
            Biotechnology = biotechnology;
        }
    }
}
