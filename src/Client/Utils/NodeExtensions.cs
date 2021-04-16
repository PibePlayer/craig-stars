using System;
using System.Collections.Generic;
using Godot;

namespace CraigStars.Utils
{
    public static class NodeExtensions
    {

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

        /// <summary>
        /// Many controls use quantity modifiers
        /// </summary>
        public static int UpdateQuantityModifer(this Node node, InputEvent @event, int quantityModifier)
        {
            if (@event is InputEventKey key)
            {
                if (key.Scancode == (uint)KeyList.Shift)
                {
                    if (key.Pressed)
                    {
                        return quantityModifier * 10;
                    }
                    else
                    {
                        return quantityModifier / 10;
                    }
                }
                else if (key.Scancode == (uint)KeyList.Control || key.Scancode == (uint)KeyList.Meta)
                {
                    if (key.Pressed)
                    {
                        return quantityModifier * 100;
                    }
                    else
                    {
                        return quantityModifier / 100;
                    }
                }
                // this quantity modifier works, but ctrl clicking stuff doesn't register...
                // log.Debug($"quantityModifier: {quantityModifier}");
            }
            return 1;

        }

    }
}

