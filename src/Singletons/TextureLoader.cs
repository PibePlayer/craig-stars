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

        public override void _Ready()
        {
            // From advice in the godot forums, this is probably a good idea.
            // It's possible that godot will use reflection to instantiate us twice
            instance = this;
        }

        public Texture FindTexture(Tech tech, int shipIndex = -1)
        {
            Texture texture;

            // try loading it from the asset path like
            // ResourceLoader.Load("res://assets/gui/tech/Engine/Alpha Drive 8.png")

            var assetPath = $"res://assets/gui/tech/{tech.Category}/{tech.Name}.png";
            texture = ResourceLoader.Load<Texture>(assetPath);

            return texture;
        }

    }
}