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


    }
}
