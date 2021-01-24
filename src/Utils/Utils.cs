using System;
using System.Collections.Generic;
using Godot;

namespace CraigStars.Utils
{
    public static class Utils
    {

        public static void Shuffle<T>(this Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static void Shuffle<T>(this Random rng, List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = list[n];
                list[n] = list[k];
                list[k] = temp;
            }
        }

        public static List<T> GetAllNodesOfType<T>(this Node node) where T : Node
        {
            List<T> nodes = new List<T>();
            foreach (Node child in node.GetChildren())
            {
                if (child is T)
                {
                    nodes.Add(child as T);
                }
                else if (child.GetChildCount() > 0)
                {
                    nodes.AddRange(child.GetAllNodesOfType<T>());
                }
            }

            return nodes;
        }

        /// <summary>
        /// Iterate over items in a list with an index
        /// i.e. items.Each((item, index) => {})
        /// </summary>
        /// <param name="ie"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        public static void Each<T>(this IEnumerable<T> ie, Action<T, int> action)
        {
            var i = 0;
            foreach (var e in ie) action(e, i++);
        }

        /// <summary>
        /// Helper function to convert a Tech into a DraggableTech
        /// </summary>
        /// <param name="tech"></param>
        /// <param name="index"></param>
        /// <returns></returns>
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

    }
}

