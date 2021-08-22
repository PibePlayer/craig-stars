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

        Task loadTask;

        public override void _Ready()
        {
            base._Ready();
            instance = this;


            loadTask = Task.Run(() =>
            {
                DefaultColumnHeaderScene = ResourceLoader.Load<PackedScene>(DefaultColumnHeaderScenePath);

                // precreate enough cells for a 10x50 table
                for (int i = 0; i < 10 * 50; i++)
                {
                    NodePool.Return<CSLabelCell>(new CSLabelCell());
                }

                for (int i = 0; i < 10; i++)
                {
                    // pre-create column headers
                    NodePool.Return<ColumnHeader>(DefaultColumnHeaderScene.Instance<ColumnHeader>());
                }
            });
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                NodePool.FreeAll<CSLabelCell>();
                NodePool.FreeAll<ColumnHeader>();
            }
        }

    }
}
