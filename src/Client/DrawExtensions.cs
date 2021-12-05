using Godot;

namespace CraigStars.Client
{
    public static class DrawExtensions
    {
        public static void DrawDiamondOutline(this Control control, Vector2 position, float size, Color color)
        {
            // draw a diamond shape
            control.DrawPolyline(new Vector2[] {
                new Vector2(position.x - size / 2, position.y),
                new Vector2(position.x, position.y - size / 2),
                new Vector2(position.x + size / 2, position.y),
                new Vector2(position.x, position.y + size / 2),
                new Vector2(position.x - size / 2, position.y),
            }, color);
        }

        public static void DrawDiamond(this Control control, Vector2 position, float size, Color color)
        {
            // draw a diamond shape
            control.DrawPolygon(new Vector2[] {
                new Vector2(position.x - size / 2, position.y),
                new Vector2(position.x, position.y - size / 2),
                new Vector2(position.x + size / 2, position.y),
                new Vector2(position.x, position.y + size / 2),
                new Vector2(position.x - size / 2, position.y),
            }, new Color[] { color });
        }

        public static void DrawCross(this Control control, Vector2 position, float size, Color color)
        {
            // draw a cross
            control.DrawLine(
                new Vector2(position.x - size / 2, position.y),
                new Vector2(position.x + size / 2, position.y),
                color
            );
            control.DrawLine(
                new Vector2(position.x, position.y - size / 2),
                new Vector2(position.x, position.y + size / 2),
                color
            );

        }
    }
}