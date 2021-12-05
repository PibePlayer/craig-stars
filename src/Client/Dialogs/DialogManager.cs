using System;
using Godot;

namespace CraigStars.Client
{
    /// <summary>
    /// Node for managing dialogs
    /// </summary>
    public class DialogManager : Node
    {
        public static int DialogRefCount { get; set; } = 0;

    }
}