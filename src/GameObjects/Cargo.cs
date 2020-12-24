using System;

public class Cargo : Mineral
{
    public Cargo(int ironium = 0, int boranium = 0, int germaninum = 0, int population = 0, int fuel = 0) : base(ironium, boranium, germaninum)
    {
        Population = population;
        Fuel = fuel;
    }

    public int Population { get; set; }
    public int Fuel { get; set; }

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