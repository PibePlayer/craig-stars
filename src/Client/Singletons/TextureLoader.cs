using Godot;
using System;
using System.Collections.Generic;

namespace CraigStars.Singletons
{
    public class TextureLoader : Node
    {
        public const int NumPlanetImages = 9;
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
            Texture texture = null;

            // try loading it from the asset path like
            // ResourceLoader.Load("res://assets/gui/tech/Engine/Alpha Drive 8.png")

            if (shipIndex != -1)
            {
                var assetPath = $"res://assets/gui/tech/{tech.Category}/{tech.Name}{shipIndex:0000}.png";
                if (ResourceLoader.Exists(assetPath))
                {
                    texture = ResourceLoader.Load<Texture>(assetPath);
                }
            }
            else
            {
                var assetPath = $"res://assets/gui/tech/{tech.Category}/{tech.Name}.png";
                if (ResourceLoader.Exists(assetPath))
                {
                    texture = ResourceLoader.Load<Texture>(assetPath);
                }
            }

            return texture;
        }

        public Texture FindTexture(TechHull hull)
        {
            return FindTexture((Tech)hull, 0);
        }


        public Texture FindTexture(ShipDesign shipDesign)
        {
            Texture texture;

            // try loading it from the asset path like
            // ResourceLoader.Load("res://assets/gui/tech/ShipDesign/Colony Ship0002.png")
            var assetPath = $"res://assets/gui/tech/{shipDesign.Hull.Category}/{shipDesign.Hull.Name}{shipDesign.HullSetNumber:0000}.png";
            texture = ResourceLoader.Load<Texture>(assetPath);

            return texture;
        }

        public Texture FindTexture(Planet planet)
        {
            // use a unique planet image based on id
            // we have 9 planet images currently
            int num = (planet.Id % (NumPlanetImages - 1)) + 1;
            var planetTextureAssetPath = $"res://assets/gui/planet/Planet0{num}.jpg";
            return ResourceLoader.Load<Texture>(planetTextureAssetPath);
        }

    }
}