
using System;
using System.Collections.Concurrent;
using Godot;

namespace CraigStarsTable
{
    public static class NodePool
    {
        static class PerType<T> where T : Node
        {
            public static ConcurrentBag<T> bag = new ConcurrentBag<T>();
        }

        public static T Get<T>(PackedScene packedScene) where T : Node
        {
            var bag = PerType<T>.bag;
            if (bag.TryTake(out T item))
            {
                return item;
            }
            else
            {
                return packedScene.Instance<T>();
            }
        }

        public static T Get<T>() where T : Node, new()
        {
            var bag = PerType<T>.bag;
            if (bag.TryTake(out T item))
            {
                return item;
            }
            else
            {
                return new T();
            }
        }

        public static void Return<T>(T item, Action<T> onReturn = null) where T : Node
        {
            var bag = PerType<T>.bag;
            onReturn?.Invoke(item);
            bag.Add(item);

            // make sure this item isn't cleaned up when the parent goes away
            // it will be freed in CSResourceLoader
            item.GetParent()?.RemoveChild(item);
        }

        public static void FreeAll<T>() where T : Node
        {
            var bag = PerType<T>.bag;
            while (bag.TryTake(out T item))
            {
                item.Free();
            }
        }

    }
}