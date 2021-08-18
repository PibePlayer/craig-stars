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


        static Task loadTask;

        public override void _Ready()
        {
            TotalResources = packedScenePaths.Count + texturePaths.Count;

            loadTask = Task.Run(() =>
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
        }

        public static PackedScene GetPackedScene(string filename)
        {
            loadTask.Wait();
            if (PackedScenes.TryGetValue(filename, out PackedScene resource))
            {
                return resource;
            }
            log.Error($"Cannot find preloaded resource for {filename}");
            return null;
        }

        public static Texture GetTexture(string filename)
        {
            loadTask.Wait();
            if (Textures.TryGetValue(filename, out Texture resource))
            {
                return resource;
            }
            log.Error($"Cannot find preloaded resource for {filename}");
            return null;
        }

    }
}