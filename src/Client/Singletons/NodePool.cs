
using System;
using System.Collections.Concurrent;
using Godot;

namespace CraigStars.Singletons
{
    public static class NodePool
    {
        static CSLog log = LogProvider.GetLogger(typeof(NodePool));
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
                // log.Debug($"Instantiating new {typeof(T)}");
                return packedScene.Instance<T>();
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

            // log.Debug($"Returned NodePool resource {typeof(T)} {item} to pool.");
        }

        public static void FreeAll<T>() where T : Node
        {
            var bag = PerType<T>.bag;
            while (bag.TryTake(out T item))
            {
                // log.Debug($"Freeing NodePool resource {typeof(T)} {item}");
                item.Free();
            }
        }

    }
}