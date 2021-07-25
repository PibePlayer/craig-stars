using Godot;

namespace CraigStars.Utils
{
    /// <summary>
    /// Provides network extensions to nodes
    /// </summary>
    public static class NetworkExtensions
    {
        /// <summary>
        /// Returns true if this scene tree is hosting a game and the server
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool IsServer(this Node node)
        {
            return node.GetTree().HasNetworkPeer() && node.GetTree().IsNetworkServer();
        }

        /// <summary>
        /// Returns true if this scene tree is a client connected to a multiplayer game
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool IsClient(this Node node)
        {
            return node.GetTree().HasNetworkPeer() && !node.GetTree().IsNetworkServer();
        }

        /// <summary>
        /// Returns true if this scene tree is the server or a single player game
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool IsServerOrSinglePlayer(this Node node)
        {
            return node.IsSinglePlayer() || node.IsServer();
        }

        /// <summary>
        /// Returns true if this scene tree is connected to a multiplayer game
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool IsMultiplayer(this Node node)
        {
            return node.GetTree().HasNetworkPeer();
        }

        /// <summary>
        /// Returns true if this scene tree is not connected to a multiplayer game and is singleplayer
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool IsSinglePlayer(this Node node)
        {
            return !node.GetTree().HasNetworkPeer();
        }

        public static int GetNetworkId(this Node node)
        {
            if (node.IsMultiplayer())
            {
                return node.GetTree().GetNetworkUniqueId();
            }
            else
            {
                // no multiplayer, treat it like a host
                return 1;
            }
        }

    }
}
