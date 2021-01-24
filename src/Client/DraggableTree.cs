using Godot;
using System;
using System.Collections.Generic;

public class DraggableTree : Tree
{
    public bool DragAndDroppable { get; set; } = false;

    /// <summary>
    /// Override GetDragData to allow the selected TreeItem to be drag and dropped
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public override object GetDragData(Vector2 position)
    {
        // if we have a selected item, let the user drag it
        var selected = GetSelected();
        if (selected != null)
        {
            // We create a new Control with a TextureRect in it. The Control is just a container
            // so we can position the TextureRect to be centered on the mouse.
            var control = new Control();
            var preview = new TextureRect()
            {
                // use whatever icon is set on the selected tree item
                Texture = selected.GetIcon(0),
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            };
            // offset the preview texture to be centered
            preview.RectPosition = new Vector2(-preview.Texture.GetWidth() / 2, -preview.Texture.GetHeight() / 2);
            control.AddChild(preview);

            // show our user the icon of the tree item they are dragging
            SetDragPreview(control);
        }

        // each of our tree items has some metadata associated with it. 
        // If we have a selected item, return its metadata so whoever we drop the
        // item on knows what it is. In the case of the TechTree, it's a serialized version of DraggableTech
        // with a name, category, and slot type
        return selected?.GetMetadata(0);
    }

}
