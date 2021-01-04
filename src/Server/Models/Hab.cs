using System;

namespace CraigStars
{
    public class Hab
    {
        public int Grav { get; set; }
        public int Temp { get; set; }
        public int Rad { get; set; }

        public Hab(int grav = 0, int temp = 0, int rad = 0)
        {
            Grav = grav;
            Temp = temp;
            Rad = rad;
        }

        /// <summary>
        /// Copy values from an existing Hab
        /// </summary>
        /// <param name="hab"></param>
        public void Copy(Hab hab)
        {
            Grav = hab.Grav;
            Temp = hab.Temp;
            Rad = hab.Rad;
        }

        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return Grav;
                    case 1:
                        return Temp;
                    case 2:
                        return Rad;
                    default:
                        throw new IndexOutOfRangeException($"Index {index} out of range for {this.GetType().ToString()}");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        Grav = value;
                        break;
                    case 1:
                        Temp = value;
                        break;
                    case 2:
                        Rad = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException($"Index {index} out of range for {this.GetType().ToString()}");
                }

            }
        }
    }
}