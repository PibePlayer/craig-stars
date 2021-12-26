using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CraigStars.Client;
using CraigStars.Singletons;
using Godot;

namespace CraigStars.Utils
{
    public static class NodeExtensions
    {
        static CSLog log = LogProvider.GetLogger(typeof(NodeExtensions));

        static Dictionary<Type, IEnumerable<FieldInfo>> fieldsForType = new();

        /// <summary>
        /// Resolve dependencies using DependencyInjection
        /// ref: https://playdivinary.com/posts/di-in-godot/
        /// </summary>
        /// <param name="node"></param>
        public static void ResolveDependencies(this Node node)
        {
            var injectionSystem = node.GetNode<DependencyInjectionSystem>("/root/DependencyInjectionSystem");
            var attribute = typeof(InjectAttribute);
            var type = node.GetType();

            if (!fieldsForType.TryGetValue(type, out var fields))
            {
                fields = type
                    .GetRuntimeFields()
                    .Where(f => f.GetCustomAttributes(attribute, true).Any());
            }

            foreach (var field in fields)
            {
                var obj = injectionSystem.Resolve(field.FieldType);
                try
                {
                    field.SetValue(node, obj);
                }
                catch (InvalidCastException)
                {
                    log.Error($"Error converting value {obj} ({obj.GetType()}) to {field.FieldType}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Change to a new scene with an initialization callback for initializing an instanced node.
        /// </summary>
        /// <param name="node">The caller node</param>
        /// <param name="nodePath">The path to the new scene</param>
        /// <param name="initCallback">A callback function to be called after the node is instanced</param>
        /// <typeparam name="T">The type of scene being instanced.</typeparam>
        public static void ChangeSceneTo<T>(this Node node, NodePath nodePath, Action<T> initCallback = null) where T : Node
        {
            // create a host specific lobby scene
            var tree = node.GetTree();
            var root = tree.Root;
            var topParent = node;
            while (topParent.GetParent() != root)
            {
                topParent = topParent.GetParent();
            }
            var sceneInstance = ResourceLoader.Load<PackedScene>(nodePath).Instance<T>();
            initCallback?.Invoke(sceneInstance);
            root.RemoveChild(topParent);
            topParent.QueueFree();

            // go to the lobby
            root.AddChild(sceneInstance);
            tree.CurrentScene = sceneInstance;
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
        /// Helper to clear all child nodes from a node
        /// </summary>
        /// <param name="node"></param>
        public static void ClearChildren(this Node node)
        {
            foreach (Node child in node.GetChildren())
            {
                node.RemoveChild(child);
                child.QueueFree();
            }
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
                        return Math.Max(1, quantityModifier / 10);
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
                        return Math.Max(1, quantityModifier / 100);
                    }
                }
                else
                {
                    return 1;
                }
                // this quantity modifier works, but ctrl clicking stuff doesn't register...
                // log.Debug($"quantityModifier: {quantityModifier}");
            }
            return quantityModifier;

        }

    }
}

