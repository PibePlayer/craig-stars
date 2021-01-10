using System;

namespace CraigStars
{
    public class Cargo : Mineral
    {
        public Cargo() { }

        public Cargo(int ironium = 0, int boranium = 0, int germanium = 0, int colonists = 0, int fuel = 0) : base(ironium, boranium, germanium)
        {
            Colonists = colonists;
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

        public override string ToString()
        {
            return $"Cargo i:{Ironium}, b:{Boranium}, g:{Germanium}, c:{Colonists}, f:{Fuel}";
        }

        /// <summary>
        /// Make a clone of this Cargo object
        /// </summary>
        /// <returns></returns>
        public Cargo Clone()
        {
            return new Cargo()
            {
                Ironium = Ironium,
                Boranium = Boranium,
                Germanium = Germanium,
                Colonists = Colonists,
                Fuel = Fuel,
            };
        }

        public static Cargo operator +(Cargo a, Mineral b)
        {
            return new Cargo(
                a.Ironium + b.Ironium,
                a.Boranium + b.Boranium,
                a.Germanium + b.Germanium
            );
        }


        public static Cargo operator +(Cargo a, Cargo b)
        {
            return new Cargo(
                a.Ironium + b.Ironium,
                a.Boranium + b.Boranium,
                a.Germanium + b.Germanium,
                a.Colonists + b.Colonists,
                a.Fuel + b.Fuel
            );
        }

        public static Cargo operator -(Cargo a, Cargo b)
        {
            return new Cargo(
                a.Ironium - b.Ironium,
                a.Boranium - b.Boranium,
                a.Germanium - b.Germanium,
                a.Colonists - b.Colonists,
                a.Fuel - b.Fuel
            );
        }

        public static Cargo operator -(Cargo a)
        {
            return new Cargo(
                -a.Ironium,
                -a.Boranium,
                -a.Germanium,
                -a.Colonists,
                -a.Fuel
            );
        }

        public static Cargo operator /(Cargo a, int num)
        {
            return new Cargo(
                a.Ironium / num,
                a.Boranium / num,
                a.Germanium / num,
                a.Colonists / num,
                a.Fuel / num
            );
        }

        public static Cargo operator *(Cargo a, float num)
        {
            return new Cargo(
                (int)(a.Ironium * num),
                (int)(a.Boranium * num),
                (int)(a.Germanium * num),
                (int)(a.Colonists * num),
                (int)(a.Fuel * num)
            );
        }

        /// <summary>
        /// Return true if all values in the cargo are greater than this number
        /// number
        /// </summary>
        /// <param name="a"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool operator >=(Cargo a, int num)
        {
            return a.Ironium >= num && a.Boranium >= num && a.Germanium >= num && a.Colonists >= num && a.Fuel >= num;
        }

        /// <summary>
        /// Return true if all values in the cargo are less than this number
        /// number
        /// </summary>
        /// <param name="a"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool operator <=(Cargo a, int num)
        {
            return a.Ironium <= num && a.Boranium <= num && a.Germanium <= num && a.Colonists <= num && a.Fuel <= num;
        }

        /// <summary>
        /// Return true if all values in the cargo are greater than this number
        /// number
        /// </summary>
        /// <param name="a"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool operator >(Cargo a, int num)
        {
            return a.Ironium > num && a.Boranium > num && a.Germanium > num && a.Colonists > num && a.Fuel > num;
        }

        /// <summary>
        /// Return true if all values in the cargo are less than this number
        /// number
        /// </summary>
        /// <param name="a"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool operator <(Cargo a, int num)
        {
            return a.Ironium < num && a.Boranium < num && a.Germanium < num && a.Colonists < num && a.Fuel < num;
        }

        public void Add(Mineral mineral)
        {
            Ironium += mineral.Ironium;
            Boranium += mineral.Boranium;
            Germanium += mineral.Germanium;
        }

        public Cost ToCost(int resources = 0)
        {
            return new Cost(Ironium, Boranium, Germanium, resources);
        }
    }
}