
using System;
using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// Intel about players 
    /// </summary>
    public class PlayerInfo
    {
        public int Num { get; set; }
        public string Name { get; set; }
        public bool Seen { get; set; }
        public string RaceName { get; set; }
        public string RacePluralName { get; set; }

        [JsonIgnore] public string KnownName { get => Seen ? RacePluralName : $"Player {Num + 1}"; }

        public PlayerInfo() { }

        public PlayerInfo(int num, string name)
        {
            Num = num;
            Name = name;
        }


        public override string ToString()
        {
            return $"Player {Num + 1} - {Name}{(RacePluralName == null ? "" : " " + RacePluralName)}";
        }
    }
}