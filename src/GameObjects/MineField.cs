using Godot;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using CraigStars.Singletons;
using System;

namespace CraigStars
{
    /// <summary>
    /// A mine field 
    /// </summary>
    public class MineField : MapObject
    {
        public int NumMines { get; set; }
        public int Radius { get; set; }

        public override void _Ready()
        {
            base._Ready();
        }

        public override void _ExitTree()
        {
            base._ExitTree();
        }

        public override void UpdateSprite()
        {

        }

    }
}
