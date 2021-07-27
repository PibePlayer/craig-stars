using Godot;

namespace CraigStars.Client
{
    public class Camera2D : Godot.Camera2D
    {
        static CSLog log = LogProvider.GetLogger(typeof(Camera2D));

        [Export]
        public float ZoomConstant { get; set; } = .05f;

        [Export]
        public float MinZoom { get; set; } = 4f;

        [Export]
        public float MaxZoom { get; set; } = .05f;

        [Export]
        public float ScrollConstant { get; set; } = 3f;

        // our current zoom level, used for updating zoom
        float currentZoomLevel = 1;

        // whether we are dragging the viewport around
        bool dragging = false;

        public override void _UnhandledInput(InputEvent @event)
        {
            if (DialogManager.DialogRefCount > 0)
            {
                // don't move the map around if we have a dialog open
                return;
            }

            if (@event.IsActionPressed("zoom_in"))
            {
                UpdateZoom(-ZoomConstant, GetLocalMousePosition());
            }
            else if (Input.IsActionPressed("zoom_out"))
            {
                UpdateZoom(ZoomConstant, GetLocalMousePosition());
            }

            if (@event is InputEventMagnifyGesture magnifyGesture)
            {
                UpdateZoom((1 - magnifyGesture.Factor) / 2, GetLocalMousePosition());
            }


            if (@event is InputEventPanGesture panGesture)
            {
                log.Debug($"panGesture delta: {panGesture.Delta}");
                Position += panGesture.Delta * Zoom * 20;
            }

            if (@event is InputEventMouseMotion eventMouseMotion && dragging)
            {
                Position += eventMouseMotion.Relative * Zoom * -1;
            }

            if (@event is InputEventMouseButton eventMouseButton && (eventMouseButton.ButtonIndex == 3 || eventMouseButton.ButtonIndex == 2))
            {
                dragging = eventMouseButton.Pressed;
            }
        }

        /// <summary>
        /// Update the zoom level, zooming into wherever the mouse cursor is
        /// </summary>
        /// <param name="increment">The amount to increment the zoom level to</param>
        /// <param name="anchor">The anchor point to zoom in on</param>
        void UpdateZoom(float increment, Vector2 anchor)
        {
            var oldZoom = currentZoomLevel;
            currentZoomLevel += increment;
            // for zoom, "max" is smaller, like .05 and min is greater, like 4
            // because of camera reasons. I don't know. Camera folk are weird
            currentZoomLevel = Mathf.Clamp(currentZoomLevel + increment, MaxZoom, MinZoom);
            if (currentZoomLevel == oldZoom)
            {
                return;
            }

            var zoomCenter = anchor - Offset;
            var ratio = 1 - currentZoomLevel / oldZoom;
            Offset += zoomCenter * ratio;
            Zoom = new Vector2(currentZoomLevel, currentZoomLevel);
        }

        /// <summary>
        /// Update the camera position based on movement keys/mouse drag
        /// </summary>
        /// <param name="delta"></param>
        public override void _Process(float delta)
        {
            if (DialogManager.DialogRefCount > 0)
            {
                return;
            }
            var scrollAmount = ScrollConstant * Zoom.x;

            if (Input.IsActionPressed("up"))
            {
                Position = new Vector2(Position.x, Position.y - scrollAmount);
            }
            if (Input.IsActionPressed("down"))
            {
                Position = new Vector2(Position.x, Position.y + scrollAmount);
            }
            if (Input.IsActionPressed("left"))
            {
                Position = new Vector2(Position.x - scrollAmount, Position.y);
            }
            if (Input.IsActionPressed("right"))
            {
                Position = new Vector2(Position.x + scrollAmount, Position.y);
            }
        }
    }
}
