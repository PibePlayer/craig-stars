using Godot;
using System;
using System.Collections.Generic;

using CraigStars.Singletons;

namespace CraigStars
{

    public abstract class MapObjectSprite<T> : Node2D where T : MapObject
    {
        
        /// <summary>
        /// Updates the sprite based on characteristics of the MapObject
        /// </summary>
        /// <param name="player"></param>
        /// <param name="mapObject"></param>
        public abstract void UpdateSprite(Player player, T mapObject);
    }
}