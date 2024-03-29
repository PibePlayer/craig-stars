using System;
using Newtonsoft.Json;

namespace CraigStars
{
    public readonly struct Hab
    {
        public readonly int grav;
        public readonly int temp;
        public readonly int rad;

        [JsonConstructor]
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

        public override string ToString()
        {
            return $"hab g:{grav}, t:{temp}, r:{rad}";
        }

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

        public int this[HabType index]
        {
            get
            {
                switch (index)
                {
                    case HabType.Gravity:
                        return grav;
                    case HabType.Temperature:
                        return temp;
                    case HabType.Radiation:
                        return rad;
                    default:
                        throw new IndexOutOfRangeException($"Index {index} out of range for {this.GetType().ToString()}");
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Hab hab)
            {
                return Equals(hab);
            }
            return false;
        }

        public bool Equals(Hab other)
        {
            return grav == other.grav && temp == other.temp && rad == other.rad;
        }

        public static bool operator ==(Hab a, Hab b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Hab a, Hab b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return grav.GetHashCode() ^ temp.GetHashCode() ^ rad.GetHashCode();
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

        public Hab WithType(HabType type, int value = 0)
        {
            switch (type)
            {
                case HabType.Gravity:
                    return WithGrav(value);
                case HabType.Temperature:
                    return WithTemp(value);
                case HabType.Radiation:
                    return WithRad(value);
                default:
                    throw new IndexOutOfRangeException($"Type {type} out of range for {this.GetType().ToString()}");
            }
        }

        public static Hab operator +(Hab a, Hab b)
        {
            return new Hab(
                a.grav + b.grav,
                a.temp + b.temp,
                a.rad + b.rad
            );
        }

        public static Hab operator -(Hab a, Hab b)
        {
            return new Hab(
                a.grav - b.grav,
                a.temp - b.temp,
                a.rad - b.rad
            );
        }

        public static Hab operator -(Hab a)
        {
            return new Hab(
                -a.grav,
                -a.temp,
                -a.rad
            );
        }

        /// <summary>
        /// Sum of all absolute values. Used for terraforming
        /// </summary>
        /// <returns></returns>
        [JsonIgnore] public int AbsSum { get => Math.Abs(grav) + Math.Abs(temp) + Math.Abs(rad); }

        /// <summary>
        /// Get the largest hab type. This is usually used in terraforming when Hab is the amoount
        /// the player can terraform, or the amount that has been terraformed, rather than an actual planet hab
        /// </summary>
        /// <returns></returns>
        public HabType AbsLargest()
        {
            int largest = 0;
            HabType largestHabType = HabType.Gravity;
            foreach (HabType habType in Enum.GetValues(typeof(HabType)))
            {
                if (Math.Abs(this[habType]) > Math.Abs(largest))
                {
                    largest = this[habType];
                    largestHabType = habType;
                }
            }

            return largestHabType;
        }


    }
}