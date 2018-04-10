using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Borders
{
    public Texture2D Texture { get; set; }
    public Vector2 LeftWall { get; set; }
    public Vector2 RightWall { get; set; }
    public Vector2 TopWall { get; set; }
    public Vector2 BottomWall { get; set; }

    public Borders()
    {
    }
}