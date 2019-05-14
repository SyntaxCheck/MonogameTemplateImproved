using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class MinimapObject
{
    public Texture2D Texture { get; set; }
    public Vector2 Position { get; set; }
    public Color Color { get; set; }
    public float Scale { get; set; }
    public float Opacity { get; set; }

    public MinimapObject()
    {
    }
}