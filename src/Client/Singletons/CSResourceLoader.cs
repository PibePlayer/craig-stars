using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CraigStars.Client;
using Godot;

namespace CraigStars.Singletons
{
    /// <summary>
    /// Utility to load packed scenes and other resources in a background thread
    /// </summary>
    public class CSResourceLoader : Node
    {
        static CSLog log = LogProvider.GetLogger(typeof(CSResourceLoader));

        public static event Action SceneLoadCompeteEvent;
        public static event Action SpriteLoadCompeteEvent;
        public static event Action<string> ResourceLoadingEvent;

        static List<string> preloadPackedScenePaths = new()
        {
            "res://addons/CraigStarsComponents/src/PlayerSavesColumnHeader.tscn",
            "res://addons/CraigStarsComponents/src/PublicGameInfosColumnHeader.tscn",
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
            "res://src/Client/Controls/CargoCell.tscn",
            "res://src/Client/GameView.tscn",
        };

        static List<string> preloadTexturePaths = new()
        {
            "res://assets/gui/icons/ArrowUp.svg",
            "res://assets/gui/icons/ArrowDown.svg",
            "res://assets/gui/icons/Close.svg",
        };
        static ConcurrentDictionary<string, PackedScene> PreloadPackedScenes { get; set; } = new();
        static ConcurrentDictionary<string, Texture> PreloadTextures { get; set; } = new();
        static HashSet<string> PreloadPackedSceneFilenames { get; set; } = preloadPackedScenePaths.Select(it => it.GetFile()).ToHashSet();
        public static int PreloadedSprites { get; set; } = 128;
        public static int TotalResources { get; set; } = preloadPackedScenePaths.Count + preloadTexturePaths.Count + PreloadedSprites;
        public static int Loaded { get; set; }

        /// <summary>
        /// PlayersManager is a singleton
        /// </summary>
        private static CSResourceLoader instance;
        public static CSResourceLoader Instance
        {
            get
            {
                return instance;
            }
        }

        public Task PreloadTask { get; set; }

        public override void _EnterTree()
        {
            base._EnterTree();
            instance = this;
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

        /// <summary>
        /// Start tasks to preload resources
        /// </summary>
        /// <returns></returns>
        public void StartPreload()
        {
            PreloadTask = Task.Run(async () =>
            {
                log.Debug("Loading scenes");
                var sceneLoadTask = Task.Run(() =>
                {
                    try
                    {
                        preloadPackedScenePaths.ForEach(path =>
                        {
                            log.Debug($"Preloading {path}");
                            ResourceLoadingEvent?.Invoke(path);
                            PreloadPackedScenes.TryAdd(path.GetFile(), ResourceLoader.Load<PackedScene>(path));
                            Loaded++;
                        });
                        preloadTexturePaths.ForEach(path =>
                        {
                            log.Debug($"Preloading {path}");
                            ResourceLoadingEvent?.Invoke(path);
                            PreloadTextures.TryAdd(path.GetFile(), ResourceLoader.Load<Texture>(path));
                            Loaded++;
                        });
                    }
                    catch (Exception e)
                    {
                        log.Error("Failed to preload scenes", e);
                    }
                });

                await sceneLoadTask;
                SceneLoadCompeteEvent?.Invoke();
                log.Debug("Populating NodePool with sprites");
                var spriteLoadTask = Task.Run(() =>
                {
                    try
                    {
                        // pre-instantiate some planets
                        var planetScene = PreloadPackedScenes["PlanetSprite.tscn"];
                        var fleetScene = PreloadPackedScenes["FleetSprite.tscn"];
                        var scannerCoverageScene = PreloadPackedScenes["ScannerCoverage.tscn"];
                        for (int i = 0; i < PreloadedSprites; i++)
                        {
                            ResourceLoadingEvent?.Invoke($"sprite {i}");
                            NodePool.Return<PlanetSprite>(planetScene.Instance<PlanetSprite>());
                            NodePool.Return<ScannerCoverage>(scannerCoverageScene.Instance<ScannerCoverage>());
                            NodePool.Return<FleetSprite>(fleetScene.Instance<FleetSprite>());
                            Loaded++;
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error("Failed to preload sprites", e);
                    }
                });
                await spriteLoadTask;
                log.Debug("Done populating NodePool with sprites");
                SpriteLoadCompeteEvent?.Invoke();
            });

        }

        public static PackedScene GetPackedScene(string filename)
        {
            Instance.PreloadTask.Wait();
            if (PreloadPackedScenes.TryGetValue(filename, out PackedScene resource))
            {
                return resource;
            }
            log.Error($"Cannot find preloaded resource for {filename}");
            return null;
        }

        public static Texture GetTexture(string filename)
        {
            Instance.PreloadTask.Wait();
            if (PreloadTextures.TryGetValue(filename, out Texture resource))
            {
                return resource;
            }
            log.Error($"Cannot find preloaded resource for {filename}");
            return null;
        }

    }
}