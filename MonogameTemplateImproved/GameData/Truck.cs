using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Truck : SpriteBase
{
    public Truck()
    {
    }

    public override SpriteType GetSpriteType()
    {
        return SpriteType.Truck;
    }
    public override bool IsMovable()
    {
        return true;
    }
}