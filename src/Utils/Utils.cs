using Godot;
using System;
using System.Collections.Generic;

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
}

