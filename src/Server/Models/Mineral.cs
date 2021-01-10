using System;

namespace CraigStars
{
    public class Mineral
    {
        public int Ironium { get; set; }
        public int Boranium { get; set; }
        public int Germanium { get; set; }

        public Mineral() { }

        public Mineral(int ironium = 0, int boranium = 0, int germanium = 0)
        {
            Ironium = ironium;
            Boranium = boranium;
            Germanium = germanium;
        }

        public Mineral(Mineral mineral)
        {
            Copy(mineral);
        }

        public static Mineral Empty { get => empty; }
        static Mineral empty = new Mineral();

        /// <summary>
        /// Copy values from an existing mineral
        /// </summary>
        /// <param name="mineral"></param>
        public void Copy(Mineral mineral)
        {
            Ironium = mineral.Ironium;
            Boranium = mineral.Boranium;
            Germanium = mineral.Germanium;
        }

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
            set
            {
                switch (index)
                {
                    case 0:
                        Ironium = value;
                        break;
                    case 1:
                        Boranium = value;
                        break;
                    case 2:
                        Germanium = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException($"Index {index} out of range for {this.GetType().ToString()}");
                }

            }
        }

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

        public void Add(int num)
        {
            Ironium += num;
            Boranium += num;
            Germanium += num;
        }

        public void Deconstruct(out int ironium, out int boranium, out int germanium)
        {
            ironium = Ironium;
            boranium = Boranium;
            germanium = Germanium;
        }
    }
}