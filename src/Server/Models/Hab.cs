using System;

namespace CraigStars
{
    public readonly struct Hab
    {
        public readonly int grav;
        public readonly int temp;
        public readonly int rad;

        public Hab(int grav = 0, int temp = 0, int rad = 0)
        {
            this.grav = grav;
            this.temp = temp;
            this.rad = rad;
        }

        /// <summary>
        /// Implicitly convert to an integer array for processing
        /// </summary>
        /// <param name="h"></param>
        public static implicit operator int[](Hab h) => new int[] { h.grav, h.temp, h.rad };

        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return grav;
                    case 1:
                        return temp;
                    case 2:
                        return rad;
                    default:
                        throw new IndexOutOfRangeException($"Index {index} out of range for {this.GetType().ToString()}");
                }
            }
        }

        /// <summary>
        /// Get a copy of this, with updated Grav
        /// </summary>
        /// <returns></returns>
        public Hab WithGrav(int grav = 0)
        {
            return new Hab(grav, this.temp, this.rad);
        }

        /// <summary>
        /// Get a copy of this, with updated Temp
        /// </summary>
        /// <returns></returns>
        public Hab WithTemp(int temp = 0)
        {
            return new Hab(this.grav, temp, this.rad);
        }

        /// <summary>
        /// Get a copy of this, with updated Rad
        /// </summary>
        /// <returns></returns>
        public Hab WithRad(int rad = 0)
        {
            return new Hab(this.grav, this.temp, rad);
        }

    }
}