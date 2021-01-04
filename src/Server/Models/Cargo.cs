namespace CraigStars
{
    public class Cargo : Mineral
    {
        public Cargo(int ironium = 0, int boranium = 0, int germaninum = 0, int population = 0, int fuel = 0) : base(ironium, boranium, germaninum)
        {
            Colonists = population;
            Fuel = fuel;
        }

        public int Colonists { get; set; }
        public int Fuel { get; set; }

        public int Total { get => Ironium + Boranium + Germanium + Colonists; }

        /// <summary>
        /// Copy values from an existing cargo
        /// </summary>
        /// <param name="cargo"></param>
        public void Copy(Cargo cargo)
        {
            base.Copy(cargo);
            Ironium = cargo.Ironium;
            Boranium = cargo.Boranium;
            Germanium = cargo.Germanium;
            Colonists = cargo.Colonists;
            Fuel = cargo.Fuel;
        }

        public static Cargo operator +(Cargo a, Mineral b)
        {
            return new Cargo(
                a.Ironium + b.Ironium,
                a.Boranium + b.Boranium,
                a.Germanium + b.Germanium
            );
        }

        public void Add(Mineral mineral)
        {
            Ironium += mineral.Ironium;
            Boranium += mineral.Boranium;
            Germanium += mineral.Germanium;
        }
    }
}