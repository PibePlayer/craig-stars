using System;
using Godot;
using System.Collections.Concurrent;

namespace GodotUtils.IO
{
    /// <summary>
    /// Static helper class for using the Godot file library with locking on path for multithreading
    /// Lock code from here: https://www.codeproject.com/Tips/1190802/File-Locking-in-a-Multi-Threaded-Environment
    /// </summary>
    public static class FileUtils
    {
        class LockObject
        {
            public int count = 0;
        }

        static ConcurrentDictionary<string, LockObject> fileLocks = new();

        /// <summary>
        /// Read a file as text
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadFile(string path)
        {
            string text = null;
            GetLock(path, () =>
            {
                using (var file = new File())
                {
                    file.Open(path, File.ModeFlags.Read).ThrowOnError();
                    text = file.GetAsText();
                    file.Close();
                }
            });

            return text;
        }

        /// <summary>
        /// Save a file as text
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static void SaveFile(string path, string text)
        {
            GetLock(path, () =>
            {
                // ensure the folder exists
                new Directory().MakeDirRecursive(path.GetBaseDir()).ThrowOnError();

                using (var file = new File())
                {
                    file.Open(path, File.ModeFlags.Write).ThrowOnError();
                    file.StoreString(text);
                    file.Close();
                }
            });
        }

        private static LockObject GetLock(string filename)
        {
            LockObject o = null;
            if (fileLocks.TryGetValue(filename.ToLower(), out o))
            {
                o.count++;
                return o;
            }
            else
            {
                o = new LockObject();
                fileLocks.TryAdd(filename.ToLower(), o);
                o.count++;
                return o;
            }
        }

        public static void GetLock(string filename, Action action)
        {
            lock (GetLock(filename))
            {
                action();
                Unlock(filename);
            }
        }

        private static void Unlock(string filename)
        {
            LockObject o = null;
            if (fileLocks.TryGetValue(filename.ToLower(), out o))
            {
                o.count--;
                if (o.count == 0)
                {
                    fileLocks.TryRemove(filename.ToLower(), out o);
                }
            }
        }

    }
}