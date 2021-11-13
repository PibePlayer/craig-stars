using System;

namespace CraigStars
{
    /// <summary>
    /// Provides rules for the current context (i.e. the game)
    /// </summary>
    public interface IProvider<T>
    {
        T Item { get; }
    }
}