using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class OnClickAnimationFactory : IAnimFactory
{
    public int NumberOfFrames { get; set; }
    public double FrameDurationMs { get; set; }
    public Texture2D SpriteSheetTexture { get; set; }

    public OnClickAnimationFactory(Texture2D texture, int numberOfFrames, double frameDurationMilliseconds)
    {
        SpriteSheetTexture = texture;
        NumberOfFrames = numberOfFrames;
        FrameDurationMs = frameDurationMilliseconds;
    }

    public SpriteSheetAnimator Build(Point mousePos)
    {
        SpriteSheetAnimator ssa = new SpriteSheetAnimator();

        ssa.Load(SpriteSheetTexture, NumberOfFrames, 1, 0, FrameDurationMs, new Vector2(mousePos.X, mousePos.Y), 1f, Color.Black);

        return ssa;
    }
}