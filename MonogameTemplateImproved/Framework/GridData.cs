using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GridData
{
    public Rectangle CellRectangle { get; set; }
    public List<SpriteBase> Sprites { get; set; }

    public GridData()
    {
    }
}