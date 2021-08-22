using CraigStars.Client;
using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CraigStars.Singletons
{
    /// <summary>
    /// Utility to load packed scenes and other resources in a background thread
    /// </summary>
    public class CSResourceLoader : Node
    {
        static CSLog log = LogProvider.GetLogger(typeof(CSResourceLoader));

        static List<string> packedScenePaths = new()
        {
            "res://src/Client/MenuScreens/Components/PlayerChooser.tscn",
            "res://src/Client/GameView.tscn",
            "res://src/Client/CommandPane/FleetCompositionTileTokensRow.tscn",
            "res://src/Client/Scanner/WaypointArea.tscn",
            "res://src/Client/Scanner/ScannerCoverage.tscn",
            "res://src/Client/Scanner/PlanetSprite.tscn",
            "res://src/Client/Scanner/FleetSprite.tscn",
            "res://src/Client/Scanner/SalvageSprite.tscn",
            "res://src/Client/Scanner/MineFieldSprite.tscn",
            "res://src/Client/Scanner/MineralPacketSprite.tscn",
            "res://src/Client/Scanner/WormholeSprite.tscn",
            "res://src/Client/Controls/MineralsCell.tscn",
            "res://src/Client/Controls/CargoCell.tscn"
        };

        static List<string> texturePaths = new()
        {
            "res://assets/gui/icons/ArrowUp.svg",
            "res://assets/gui/icons/ArrowDown.svg",
            "res://assets/gui/icons/Close.svg",
        };
        static Dictionary<string, PackedScene> PackedScenes { get; set; } = new();
        static Dictionary<string, Texture> Textures { get; set; } = new();
        public static int TotalResources { get; set; }
        public static int Loaded { get; set; }


        static Task sceneLoadTask;
        static Task spriteLoadTask;

        public async override void _Ready()
        {
            int preloadedSprites = 128;
            TotalResources = packedScenePaths.Count + texturePaths.Count + preloadedSprites;

            sceneLoadTask = Task.Run(() =>
            {
                packedScenePaths.ForEach(path =>
                {
                    log.Debug($"Preloading {path}");
                    PackedScenes.Add(path.GetFile(), ResourceLoader.Load<PackedScene>(path));
                    Loaded++;
                });
                texturePaths.ForEach(path =>
                {
                    log.Debug($"Preloading {path}");
                    Textures.Add(path.GetFile(), ResourceLoader.Load<Texture>(path));
                    Loaded++;
                });
            });
            
            await sceneLoadTask;
            spriteLoadTask = Task.Run(() =>
            {
                // pre-instantiate some planets
                var planetScene = PackedScenes["PlanetSprite.tscn"];
                var fleetScene = PackedScenes["FleetSprite.tscn"];
                var scannerCoverageScene = PackedScenes["ScannerCoverage.tscn"];
                for (int i = 0; i < preloadedSprites; i++)
                {
                    NodePool.Return<PlanetSprite>(planetScene.Instance<PlanetSprite>());
                    NodePool.Return<ScannerCoverage>(scannerCoverageScene.Instance<ScannerCoverage>());
                    NodePool.Return<FleetSprite>(fleetScene.Instance<FleetSprite>());
                    Loaded++;
                }
            });
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                NodePool.FreeAll<PlanetSprite>();
                NodePool.FreeAll<FleetSprite>();
                NodePool.FreeAll<WormholeSprite>();
                NodePool.FreeAll<MineralPacketSprite>();
                NodePool.FreeAll<MineFieldSprite>();
                NodePool.FreeAll<SalvageSprite>();
                NodePool.FreeAll<ScannerCoverage>();
            }
        }

        public static PackedScene GetPackedScene(string filename)
        {
            sceneLoadTask.Wait(TimeSpan.FromSeconds(5));
            if (PackedScenes.TryGetValue(filename, out PackedScene resource))
            {
                return resource;
            }
            log.Error($"Cannot find preloaded resource for {filename}");
            return null;
        }

        public static Texture GetTexture(string filename)
        {
            sceneLoadTask.Wait(TimeSpan.FromSeconds(5));
            if (Textures.TryGetValue(filename, out Texture resource))
            {
                return resource;
            }
            log.Error($"Cannot find preloaded resource for {filename}");
            return null;
        }

    }
}