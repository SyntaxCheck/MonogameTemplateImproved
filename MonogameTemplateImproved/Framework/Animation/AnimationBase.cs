using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AnimationBase
{
    public bool IsAnimationComplete { get; set; }

    public AnimationBase()
    {
    }

    public virtual void Update(GameTime gameTime)
    {

    }
    public virtual void Draw(SpriteBatch spriteBatch)
    {

    }
}