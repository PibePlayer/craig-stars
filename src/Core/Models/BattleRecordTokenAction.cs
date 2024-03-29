using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// Action for a token
    /// </summary>
    public abstract class BattleRecordTokenAction
    {
        public BattleRecordTokenAction() { }

        public BattleRecordTokenAction(BattleRecordToken token, Vector2 from)
        {
            Token = token;
            From = from;
        }

        /// <summary>
        /// Used for deserializing
        /// </summary>
        /// <returns></returns>
        public string Type { get => GetType().Name; }

        /// <summary>
        /// The token taking the action
        /// </summary>
        [JsonProperty(IsReference = true)]
        public BattleRecordToken Token { get; set; }

        /// <summary>
        /// The location of the token
        /// </summary>
        public Vector2 From { get; set; }

    }
}