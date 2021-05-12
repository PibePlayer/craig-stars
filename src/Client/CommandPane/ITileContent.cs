using System;

namespace CraigStars
{
    public delegate void UpdateTitleAction(string title);
    public delegate void UpdateVisibilityAction(bool visible);

    /// <summary>
    /// Tile sub-controls implement this interface if they need to update the Title of the tile
    /// the tile 
    /// </summary>
    public interface ITileContent
    {


        event UpdateTitleAction UpdateTitleEvent;
        event UpdateVisibilityAction UpdateVisibilityEvent;
    }
}