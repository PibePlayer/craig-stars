using System;
using Newtonsoft.Json;

namespace CraigStars
{
    public readonly struct Mineral
    {
        public readonly int Ironium;
        public readonly int Boranium;
        public readonly int Germanium;

        [JsonConstructor]
        public Mineral(int ironium = 0, int boranium = 0, int germanium = 0)
        {
            Ironium = ironium;
            Boranium = boranium;
            Germanium = germanium;
        }

        public static Mineral Empty { get => empty; }
        static Mineral empty = new Mineral();

        public static implicit operator int[](Mineral m) => new int[] { m.Ironium, m.Boranium, m.Germanium };
        public static implicit operator Mineral(int[] m) => new Mineral(m[0], m[1], m[2]);

        public override string ToString()
        {
            return $"Mineral i:{Ironium}, b:{Boranium}, g:{Germanium}";
        }

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
                    default:
                        throw new IndexOutOfRangeException($"Index {index} out of range for {this.GetType().ToString()}");
                }
            }
        }

        public int this[MineralType type] { get => this[(int)type]; }

        public static Mineral operator +(Mineral a, Mineral b)
        {
            return new Mineral(
                a.Ironium + b.Ironium,
                a.Boranium + b.Boranium,
                a.Germanium + b.Germanium
            );
        }

        public static Mineral operator /(Mineral a, int num)
        {
            return new Mineral(
                a.Ironium / num,
                a.Boranium / num,
                a.Germanium / num
            );
        }

        public static Mineral operator +(Mineral a, int num)
        {
            return new Mineral(a.Ironium + num, a.Boranium + num, a.Germanium + num);
        }

        public void Deconstruct(out int ironium, out int boranium, out int germanium)
        {
            ironium = Ironium;
            boranium = Boranium;
            germanium = Germanium;
        }

        public override bool Equals(object obj)
        {
            if (obj is Mineral mineral)
            {
                return Equals(mineral);
            }
            return false;
        }

        public bool Equals(Mineral other)
        {
            return Ironium == other.Ironium && Boranium == other.Boranium && Germanium == other.Germanium;
        }

        public static bool operator ==(Mineral a, Mineral b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Mineral a, Mineral b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return Ironium.GetHashCode() ^ Boranium.GetHashCode() ^ Germanium.GetHashCode();
        }

        /// <summary>
        /// Get a copy of this, with updated Ironium
        /// </summary>
        /// <returns></returns>
        public Mineral WithIronium(int ironium = 0)
        {
            return new Mineral(ironium, Boranium, Germanium);
        }

        /// <summary>
        /// Get a copy of this, with updated Boranium
        /// </summary>
        /// <returns></returns>
        public Mineral WithBoranium(int boranium = 0)
        {
            return new Mineral(Ironium, boranium, Germanium);
        }

        /// <summary>
        /// Get a copy of this, with updated Germanium
        /// </summary>
        /// <returns></returns>
        public Mineral WithGermanium(int germanium = 0)
        {
            return new Mineral(Ironium, Boranium, germanium);
        }

        /// <summary>
        /// Get a copy of this struct with 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Mineral WithType(MineralType type, int value = 0)
        {
            switch (type)
            {
                case MineralType.Ironium:
                    return WithIronium(value);
                case MineralType.Boranium:
                    return WithBoranium(value);
                case MineralType.Germanium:
                    return WithGermanium(value);
                default:
                    throw new IndexOutOfRangeException($"Type {type} out of range for {this.GetType().ToString()}");
            }
        }        
    }
}