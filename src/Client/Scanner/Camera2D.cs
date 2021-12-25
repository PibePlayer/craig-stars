using CraigStars.Singletons;
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
        public float MaxZoom { get; set; } = .25f;

        [Export]
        public float ScrollConstant { get; set; } = 5f;

        // our current zoom level, used for updating zoom
        float currentZoomLevel = 1;

        // whether we are dragging the viewport around
        bool dragging = false;

        /// <summary>
        /// The margin, in pixels, for the universe. This is how far past the universe border we 
        /// will let the camera pan. Note, this is constant and not impacted by zoom level
        /// </summary>
        float UniverseMargin { get; set; } = 50;

        /// <summary>
        /// The bounds of this universe. A Small universe is an 800 l.y. square, so default to that but leave some wiggle room for the
        /// camera
        /// </summary>
        Rect2 UniverseBounds { get; set; } = new Rect2(0, 0, 800, 800);

        // Font font;

        public override void _Ready()
        {
            base._Ready();
            SetProcess(false);

            // font = new Control().GetFont("font");

            Connect("visibility_changed", this, nameof(OnVisibilityChanged));

            EventManager.CenterViewOnMapObjectEvent += OnCenterViewOnMapObject;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);

            if (what == NotificationPredelete)
            {
                EventManager.CenterViewOnMapObjectEvent -= OnCenterViewOnMapObject;
            }
        }

        void OnVisibilityChanged()
        {
            if (IsVisibleInTree() && PlayersManager.GameInfo != null)
            {
                var area = PlayersManager.GameInfo.Rules.GetArea(PlayersManager.GameInfo.Size);
                UniverseBounds = new Rect2(0, 0, area);
            }
        }

        void OnCenterViewOnMapObject(MapObject mo)
        {
            CenterCamera(mo.Position);
        }

        void CenterCamera(Vector2 position)
        {
            var viewportWidth = GetViewport().Size.x;
            var viewportHeight = GetViewport().Size.y;

            // with zoom applied, the universe stays the same size, but the viewport shows less of the universe when
            // zoomed in, so it's like the view is smaller
            var viewportZoomed = new Vector2(
                viewportWidth * Zoom.x,
                viewportHeight * Zoom.y
            );

            // center this position in the viewport
            // remove the offset (by subtracting it) from the position before centering it
            UpdatePosition(position - Offset - viewportZoomed / 2);
        }

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

            if (@event.IsActionPressed("viewport_reset_zoom"))
            {
                currentZoomLevel = 1;
                Zoom = new Vector2(1, 1);
                UpdatePosition(new Vector2(0, 0));
            }

            if (@event is InputEventMagnifyGesture magnifyGesture)
            {
                UpdateZoom((1 - magnifyGesture.Factor) / 2, GetLocalMousePosition());
            }


            if (@event is InputEventPanGesture panGesture)
            {
                log.Debug($"panGesture delta: {panGesture.Delta}");
                UpdatePosition(Position + panGesture.Delta * Zoom * 20);
            }

            if (@event is InputEventMouseMotion eventMouseMotion && dragging)
            {
                UpdatePosition(Position + eventMouseMotion.Relative * Zoom * -1);
            }

            if (@event is InputEventMouseButton eventMouseButton && (eventMouseButton.ButtonIndex == 3 || eventMouseButton.ButtonIndex == 2))
            {
                dragging = eventMouseButton.Pressed;
            }

            // turn on processing if we are moving the camera
            if (Input.IsActionJustPressed("up")
                || Input.IsActionJustPressed("down")
                || Input.IsActionJustPressed("left")
                || Input.IsActionJustPressed("right")
                || Input.IsActionPressed("up")
                || Input.IsActionPressed("down")
                || Input.IsActionPressed("left")
                || Input.IsActionPressed("right")
                )
            {
                SetProcess(true);
            }
            else if (Input.IsActionJustReleased("up")
                || Input.IsActionJustReleased("down")
                || Input.IsActionJustReleased("left")
                || Input.IsActionJustReleased("right")
                && !(
                Input.IsActionPressed("up")
                || Input.IsActionPressed("down")
                || Input.IsActionPressed("left")
                || Input.IsActionPressed("right")
                )
            )
            {
                // turn off processing if we released a map move button and we aren't holding it down anymore
                SetProcess(false);
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

            // make sure we snap our position to the bounds of the camera after zoom
            UpdatePosition(Position);
        }

        /// <summary>
        /// Update the camera offset, constraining it to our bounds
        /// </summary>
        /// <param name="position"></param>
        void UpdatePosition(Vector2 position)
        {
            var viewportWidth = GetViewport().Size.x;
            var viewportHeight = GetViewport().Size.y;

            // with zoom applied, the universe stays the same size, but the viewport shows less of the universe when
            // zoomed in, so it's like the view is smaller
            var viewportZoomed = new Vector2(
                viewportWidth * Zoom.x,
                viewportHeight * Zoom.y
            );

            var maxPosition = new Vector2(
                Mathf.Max(0, UniverseBounds.Size.x - viewportZoomed.x + UniverseMargin),
                Mathf.Max(0, UniverseBounds.Size.y - viewportZoomed.y + UniverseMargin)
            );

            // make sure we don't scroll more than -50px to the left and top (the math is just for completeness, in case we had a universe that wasn't 
            // origined at 0,0 in the top left corner)
            var minPosition = new Vector2(
                UniverseBounds.Position.x / Zoom.x - UniverseMargin,
                UniverseBounds.Position.y / Zoom.y - UniverseMargin
            );

            // Update the position, but clamp it to the min/max position
            // also, apply the current Offset that happened during zoom. The offset takes is just a plain x,y offset
            // applied to the camera after everything else, so subtract the offset to remove it
            Position = new Vector2(
                Mathf.Clamp(position.x, minPosition.x - Offset.x, maxPosition.x - Offset.x),
                Mathf.Clamp(position.y, minPosition.y - Offset.y, maxPosition.y - Offset.y)
            );

            // Update();
        }

        // Leaving this commented out in case I need to debug the universe area/camera again
        // public override void _Draw()
        // {
        //     base._Draw();

        //     var viewportWidth = GetViewport().Size.x;
        //     var viewportHeight = GetViewport().Size.y;

        //     var viewportZoom = new Vector2(
        //         viewportWidth * Zoom.x,
        //         viewportHeight * Zoom.y
        //     );

        //     var maxPosition = new Vector2(
        //         Mathf.Max(0, UniverseBounds.Size.x - viewportZoom.x + UniverseMargin),
        //         Mathf.Max(0, UniverseBounds.Size.y - viewportZoom.y + UniverseMargin)
        //     );

        //     DrawString(font, Offset + new Vector2(0, font.GetStringSize("0").y + 5) * 1, $"Viewport: ({viewportWidth}, {viewportHeight})");
        //     DrawString(font, Offset + new Vector2(0, font.GetStringSize("0").y + 5) * 2, $"Viewport Zoomed: ({viewportZoom.x}, {viewportZoom.y})");
        //     DrawString(font, Offset + new Vector2(0, font.GetStringSize("0").y + 5) * 3, $"Max Position: ({maxPosition.x}, {maxPosition.y})");
        //     DrawString(font, Offset + new Vector2(0, font.GetStringSize("0").y + 5) * 4, $"Position: ({Position.x}, {Position.y})");
        //     DrawString(font, Offset + new Vector2(0, font.GetStringSize("0").y + 5) * 5, $"Offset: ({Offset.x}, {Offset.y})");
        // }

        /// <summary>
        /// Update the camera position based on movement keys/mouse drag
        /// </summary>
        /// <param name="delta"></param>
        public override void _Process(float delta)
        {
            if (DialogManager.DialogRefCount > 0)
            {
                SetProcess(false);
                return;
            }
            var scrollAmount = ScrollConstant * Zoom.x;

            if (Input.IsActionPressed("up"))
            {
                UpdatePosition(new Vector2(Position.x, Position.y - scrollAmount));
            }
            if (Input.IsActionPressed("down"))
            {
                UpdatePosition(new Vector2(Position.x, Position.y + scrollAmount));
            }
            if (Input.IsActionPressed("left"))
            {
                UpdatePosition(new Vector2(Position.x - scrollAmount, Position.y));
            }
            if (Input.IsActionPressed("right"))
            {
                UpdatePosition(new Vector2(Position.x + scrollAmount, Position.y));
            }
        }
    }
}
