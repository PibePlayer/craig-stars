using System;
using Newtonsoft.Json;

namespace CraigStars
{
    public readonly struct Cargo
    {
        public readonly int Ironium;
        public readonly int Boranium;
        public readonly int Germanium;
        public readonly int Colonists;

        [JsonConstructor]
        public Cargo(int ironium = 0, int boranium = 0, int germanium = 0, int colonists = 0)
        {
            Ironium = ironium;
            Boranium = boranium;
            Germanium = germanium;
            Colonists = colonists;
        }

        public int Total { get => Ironium + Boranium + Germanium + Colonists; }
        public bool HasColonists { get => Colonists > 0; }
        public bool HasMinerals { get => Ironium > 0 || Boranium > 0 || Germanium > 0; }

        /// <summary>
        /// A cargo can be converted to a cost, just sharing the mineral component
        /// </summary>
        /// <param name="cargo"></param>
        public static implicit operator Cost(Cargo cargo) => new Cost(cargo.Ironium, cargo.Boranium, cargo.Germanium);
        public static implicit operator Cargo(Cost cost) => new Cargo(cost.Ironium, cost.Boranium, cost.Germanium);
        public static implicit operator Cargo(Mineral mineral) => new Cargo(mineral.Ironium, mineral.Boranium, mineral.Germanium);

        public override string ToString()
        {
            return $"Cargo i:{Ironium}, b:{Boranium}, g:{Germanium}, c:{Colonists}";
        }

        public static Cargo Empty { get => empty; }
        static Cargo empty = new Cargo();

        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return Ironium;
                    case 1:
                        return Boranium;
                    case 2:
                        return Germanium;
                    case 3:
                        return Colonists;
                    default:
                        throw new IndexOutOfRangeException($"Index {index} out of range for {this.GetType().ToString()}");
                }
            }
        }

        public int this[CargoType type] { get => this[(int)type]; }
        public int this[MineralType type] { get => this[(int)type]; }

        public static Cargo OfAmount(CargoType type, int amount)
        {
            switch (type)
            {
                case CargoType.Ironium:
                    return new Cargo(ironium: amount);
                case CargoType.Boranium:
                    return new Cargo(boranium: amount);
                case CargoType.Germanium:
                    return new Cargo(germanium: amount);
                case CargoType.Colonists:
                    return new Cargo(colonists: amount);
                default:
                    throw new InvalidOperationException($"CargoType {type} not allowed");
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Cargo cargo)
            {
                return Equals(cargo);
            }
            return false;
        }

        public bool Equals(Cargo other)
        {
            return Ironium == other.Ironium && Boranium == other.Boranium && Germanium == other.Germanium && Colonists == other.Colonists;
        }

        public static bool operator ==(Cargo a, Cargo b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Cargo a, Cargo b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return Ironium.GetHashCode() ^ Boranium.GetHashCode() ^ Germanium.GetHashCode() ^ Colonists.GetHashCode();
        }

        public static Cargo operator +(Cargo a, Mineral b)
        {
            return new Cargo(
                a.Ironium + b.Ironium,
                a.Boranium + b.Boranium,
                a.Germanium + b.Germanium,
                a.Colonists
            );
        }


        public static Cargo operator +(Cargo a, Cargo b)
        {
            return new Cargo(
                a.Ironium + b.Ironium,
                a.Boranium + b.Boranium,
                a.Germanium + b.Germanium,
                a.Colonists + b.Colonists
            );
        }

        public static Cargo operator -(Cargo a, Cargo b)
        {
            return new Cargo(
                a.Ironium - b.Ironium,
                a.Boranium - b.Boranium,
                a.Germanium - b.Germanium,
                a.Colonists - b.Colonists
            );
        }

        public static Cargo operator -(Cargo a)
        {
            return new Cargo(
                -a.Ironium,
                -a.Boranium,
                -a.Germanium,
                -a.Colonists
            );
        }

        public static Cargo operator /(Cargo a, int num)
        {
            return new Cargo(
                a.Ironium / num,
                a.Boranium / num,
                a.Germanium / num,
                a.Colonists / num
            );
        }

        public static Cargo operator *(Cargo a, float num)
        {
            return new Cargo(
                (int)(a.Ironium * num),
                (int)(a.Boranium * num),
                (int)(a.Germanium * num),
                (int)(a.Colonists * num)
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
            return a.Ironium >= num && a.Boranium >= num && a.Germanium >= num && a.Colonists >= num;
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
            return a.Ironium <= num && a.Boranium <= num && a.Germanium <= num && a.Colonists <= num;
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
            return a.Ironium > num && a.Boranium > num && a.Germanium > num && a.Colonists > num;
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
            return a.Ironium < num && a.Boranium < num && a.Germanium < num && a.Colonists < num;
        }

        public Cost ToCost(int resources = 0)
        {
            return new Cost(Ironium, Boranium, Germanium, resources);
        }

        public Mineral ToMineral()
        {
            return new Mineral(Ironium, Boranium, Germanium);
        }

        /// <summary>
        /// Get a copy of this, with updated Ironium
        /// </summary>
        /// <returns></returns>
        public Cargo WithIronium(int ironium = 0)
        {
            return new Cargo(ironium, Boranium, Germanium, Colonists);
        }

        /// <summary>
        /// Get a copy of this, with updated Boranium
        /// </summary>
        /// <returns></returns>
        public Cargo WithBoranium(int boranium = 0)
        {
            return new Cargo(Ironium, boranium, Germanium, Colonists);
        }

        /// <summary>
        /// Get a copy of this, with updated Germanium
        /// </summary>
        /// <returns></returns>
        public Cargo WithGermanium(int germanium = 0)
        {
            return new Cargo(Ironium, Boranium, germanium, Colonists);
        }

        /// <summary>
        /// Get a copy of this, with updated Colonists
        /// </summary>
        /// <returns></returns>
        public Cargo WithColonists(int colonists = 0)
        {
            return new Cargo(Ironium, Boranium, Germanium, colonists);
        }
    }
}