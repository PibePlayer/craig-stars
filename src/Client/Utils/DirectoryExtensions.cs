using System;
using System.Collections.Generic;
using Godot;

namespace CraigStars.Utils
{
    public static class DirectoryExtensions
    {

        /// <summary>
        /// Recursively delete a directory and all its files
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="path"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public static Error Remove(this Directory dir, string path, bool recursive = false)
        {
            if (recursive == false)
            {
                return dir.Remove(path);
            }
            else
            {
                using (var directory = new Directory())
                {
                    directory.Open(path);
                    directory.ListDirBegin(skipNavigational: true);
                    while (true)
                    {
                        string file = directory.GetNext();
                        if (file == null || file.Empty())
                        {
                            break;
                        }
                        if (directory.CurrentIsDir())
                        {
                            // recurse into subfolder
                            var subFolder = new Directory();
                            var err = subFolder.Remove(path + "/" + file, true);
                            if (err != Error.Ok)
                            {
                                return err;
                            }
                        }
                        else
                        {
                            // remove the file
                            var err = directory.Remove(path + "/" + file);
                            if (err != Error.Ok)
                            {
                                return err;
                            }
                        }
                    }
                }
            }

            return dir.Remove(path);
        }

    }
}

