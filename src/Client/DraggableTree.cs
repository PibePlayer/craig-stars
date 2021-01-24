using Godot;
using System;
using System.Collections.Generic;

public class DraggableTree : Tree
{
    public bool DragAndDroppable { get; set; } = false;

    public override object GetDragData(Vector2 position)
    {
        var selected = GetSelected();
        if (selected != null)
        {
            var preview = new TextureRect()
            {
                Texture = selected.GetIcon(0),
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
            };
            SetDragPreview(preview);
        }

        return selected?.GetMetadata(0);
    }

}
