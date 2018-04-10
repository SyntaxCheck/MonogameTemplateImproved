using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Van : SpriteBase
{
    public Van()
    {
    }

    public override SpriteType GetSpriteType()
    {
        return SpriteType.Van;
    }
    public override bool IsMovable()
    {
        return true;
    }
}