using Godot;
using System;
using System.Collections.Generic;

namespace CraigStars.Singletons
{
    public class TextureLoader : Node
    {
        /// <summary>
        /// PlayersManager is a singleton
        /// </summary>
        private static TextureLoader instance;
        public static TextureLoader Instance
        {
            get
            {
                return instance;
            }
        }

        Dictionary<string, Texture> texturesByName = new Dictionary<string, Texture>();

        public override void _Ready()
        {
            // From advice in the godot forums, this is probably a good idea.
            // It's possible that godot will use reflection to instantiate us twice
            instance = this;
        }

        public Texture FindTechTexture(string name, TechCategory category = TechCategory.All, int shipIndex = -1)
        {
            Texture texture;

            var key = $"{category.ToString()}/{name}{(shipIndex != -1 ? shipIndex.ToString() : "")}";
            if (texturesByName.TryGetValue(key, out texture))
            {
                return texture;
            }

            // try loading it from the asset path like
            // ResourceLoader.Load("res://Assets/GUI/Tech/Engine/Alpha Drive 8.png")

            var assetPath = $"res://Assets/GUI/Tech/{category}/{name}.png";
            texture = ResourceLoader.Load<Texture>(assetPath);
            if (texture != null)
            {
                texturesByName[key] = texture;
            }

            return texture;
        }

    }
}