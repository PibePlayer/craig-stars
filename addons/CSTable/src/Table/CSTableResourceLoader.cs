using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Godot;

namespace CraigStarsTable
{
    /// <summary>
    /// A singleton node for creating new Server scene trees when multiplayer games are hosted
    /// </summary>
    public class CSTableResourceLoader : Node
    {

        public const string DefaultColumnHeaderScenePath = "res://addons/CSTable/src/Table/ColumnHeader.tscn";

        public PackedScene DefaultColumnHeaderScene { get; set; }

        /// <summary>
        /// Server is a singleton
        /// </summary>
        private static CSTableResourceLoader instance;
        public static CSTableResourceLoader Instance
        {
            get
            {
                return instance;
            }
        }

        CSTableResourceLoader()
        {
            instance = this;
        }

        public static int TotalResources { get; set; }
        public static int Loaded { get; set; }

        static Task sceneLoadTask;
        static Task spriteLoadTask;

        public override void _Ready()
        {
            base._Ready();
            instance = this;

            int numCells = 10 * 50;
            int numHeaders = 10;

            TotalResources = 1 + numCells + numHeaders;
            Loaded = 0;

            DefaultColumnHeaderScene = ResourceLoader.Load<PackedScene>(DefaultColumnHeaderScenePath);
            Loaded++;

            spriteLoadTask = Task.Run(() =>
            {
                // precreate enough cells for a 10x50 table
                for (int i = 0; i < numCells; i++)
                {
                    CSTableNodePool.Return<CSLabelCell>(new CSLabelCell());
                    Loaded++;
                }

                for (int i = 0; i < numHeaders; i++)
                {
                    // pre-create column headers
                    CSTableNodePool.Return<ColumnHeader>(DefaultColumnHeaderScene.Instance<ColumnHeader>());
                    Loaded++;
                }
            });

        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                CSTableNodePool.FreeAll<CSLabelCell>();
                CSTableNodePool.FreeAll<ColumnHeader>();
            }
        }

    }
}
