using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace CraigStars.Utils
{
    public static class GodotSerializers
    {
        /// <summary>
        /// Serialize a Player to an array for sending over RPC
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Godot.Collections.Array ToArray(this Player p)
        {
            return new Godot.Collections.Array {
                p.NetworkId,
                p.Num,
                p.Name,
                p.Ready,
                p.AIControlled,
                p.Color,
            };
        }

        /// <summary>
        /// Update a Player from an array of serialized data
        /// </summary>
        /// <param name="p"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Player FromArray(this Player p, Godot.Collections.Array data)
        {
            int i = 0;
            p.NetworkId = (int)data[i++];
            p.Num = (int)data[i++];
            p.Name = (string)data[i++];
            p.Ready = (Boolean)data[i++];
            p.AIControlled = (Boolean)data[i++];
            p.Color = (Color)data[i++];

            return p;
        }

        public static Godot.Collections.Array ToArray(this PlayerMessage m)
        {
            return new Godot.Collections.Array {
                m.PlayerNum,
                m.Message
            };
        }

        public static PlayerMessage FromArray(this PlayerMessage m, Godot.Collections.Array data)
        {
            int i = 0;
            m.PlayerNum = (int)data[i++];
            m.Message = (string)data[i++];
            return m;
        }

        public static DraggableTech GetDraggableTech(this Tech tech, int index)
        {
            if (tech is TechHullComponent hullComponent)
            {
                return new DraggableTech(tech.Name, tech.Category, index, hullComponent.HullSlotType);
            }
            else
            {
                return new DraggableTech(tech.Name, tech.Category, index);
            }
        }

        public static Godot.Collections.Array ToArray(this DraggableTech tech)
        {
            return new Godot.Collections.Array() {
                tech.name,
                tech.category,
                tech.index,
                tech.hullSlotType
            };
        }

        public static DraggableTech FromArray(Godot.Collections.Array data)
        {
            // TODO: this is unsafe.
            return new DraggableTech(
                (string)data[0],
                (TechCategory)data[1],
                (int)data[2],
                (HullSlotType)data[3]
            );
        }

    }
}
