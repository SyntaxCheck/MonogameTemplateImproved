using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CollisionParticleAnimationFactory : IAnimFactory
{
    public int NumberOfFrames { get; set; }
    public double FrameDurationMs { get; set; }
    public Texture2D SpriteSheetTextureEastBottom { get; set; }
    public Texture2D SpriteSheetTextureEastTop { get; set; }
    public Texture2D SpriteSheetTextureWestBottom { get; set; }
    public Texture2D SpriteSheetTextureWestTop { get; set; }
    public Texture2D SpriteSheetTextureNorthLeft { get; set; }
    public Texture2D SpriteSheetTextureNorthRight { get; set; }
    public Texture2D SpriteSheetTextureSouthLeft { get; set; }
    public Texture2D SpriteSheetTextureSouthRight { get; set; }

    public CollisionParticleAnimationFactory(Texture2D eastBottom, Texture2D eastTop, Texture2D westBottom, Texture2D westTop, Texture2D northLeft, Texture2D northRight, Texture2D southLeft, Texture2D southRight, int numberOfFrames, double frameDurationMilliseconds)
    {
        SpriteSheetTextureEastBottom = eastBottom;
        SpriteSheetTextureEastTop = eastTop;
        SpriteSheetTextureWestBottom = westBottom;
        SpriteSheetTextureWestTop = westTop;
        SpriteSheetTextureNorthLeft = northLeft;
        SpriteSheetTextureNorthRight = northRight;
        SpriteSheetTextureSouthLeft = southLeft;
        SpriteSheetTextureSouthRight = southRight;
        NumberOfFrames = numberOfFrames;
        FrameDurationMs = frameDurationMilliseconds;
    }

    //Build collision animation on either side of the contact surface scaled to the rec1 scale.
    public List<SpriteSheetAnimator> Build(Rectangle rec1, Rectangle rec2, Vector2 collisionOffset, Vector2 obj1Direction, float rec1Scale, Color color)
    {
        List<SpriteSheetAnimator> rtnAnims = new List<SpriteSheetAnimator>();

        SpriteSheetAnimator anim1 = new SpriteSheetAnimator();
        SpriteSheetAnimator anim2 = new SpriteSheetAnimator();

        //Reduce scale as we get above 1f since it starts to look strange
        if (rec1Scale > 1)
        {
            float overage = rec1Scale - 1f;
            overage = overage * .5f;

            rec1Scale = 1f + overage;
        }

        if (Math.Abs(collisionOffset.X) < Math.Abs(collisionOffset.Y)) //Left or Right side collision
        {
            if (obj1Direction.X > 0) //Object 1 Right side collision
            {
                if (rec1.Top > rec2.Top)
                {
                    anim1.Load(SpriteSheetTextureWestTop, NumberOfFrames, FrameDurationMs, new Vector2(0, 0), rec1Scale, color);

                    float x = rec2.Left - (anim1.Bounds.Width / 2);
                    float y = rec1.Top - (anim1.Bounds.Height / 2);

                    anim1.Position = new Vector2(x, y);

                    rtnAnims.Add(anim1);
                }
                if (rec1.Bottom < rec2.Bottom)
                {
                    anim2.Load(SpriteSheetTextureWestBottom, NumberOfFrames, FrameDurationMs, new Vector2(0, 0), rec1Scale, color);

                    float x = rec2.Left - (anim2.Bounds.Width / 2);
                    float y = rec1.Bottom + (anim2.Bounds.Height / 2);

                    anim2.Position = new Vector2(x, y);

                    rtnAnims.Add(anim2);
                }
            }
            else //Object 1 Left side collision
            {
                if (rec1.Top > rec2.Top)
                {
                    anim1.Load(SpriteSheetTextureEastTop, NumberOfFrames, FrameDurationMs, new Vector2(0, 0), rec1Scale, color);

                    float x = rec2.Right + (anim1.Bounds.Width / 2);
                    float y = rec1.Top - (anim1.Bounds.Height / 2);

                    anim1.Position = new Vector2(x, y);

                    rtnAnims.Add(anim1);
                }
                if (rec1.Bottom < rec2.Bottom)
                {
                    anim2.Load(SpriteSheetTextureEastBottom, NumberOfFrames, FrameDurationMs, new Vector2(0, 0), rec1Scale, color);

                    float x = rec2.Right + (anim2.Bounds.Width / 2);
                    float y = rec1.Bottom + (anim2.Bounds.Height / 2);

                    anim2.Position = new Vector2(x, y);

                    rtnAnims.Add(anim2);
                }
            }
        }
        else //Top or Bottom side collision
        {
            if (obj1Direction.Y > 0) //Object 1 Bottom side collision
            {
                if (rec1.Left > rec2.Left)
                {
                    anim1.Load(SpriteSheetTextureNorthLeft, NumberOfFrames, FrameDurationMs, new Vector2(0, 0), rec1Scale, color);

                    float x = rec1.Left - (anim1.Bounds.Width / 2);
                    float y = rec2.Top - (anim1.Bounds.Height / 2);

                    anim1.Position = new Vector2(x, y);

                    rtnAnims.Add(anim1);
                }
                if (rec1.Right < rec2.Right)
                {
                    anim2.Load(SpriteSheetTextureNorthRight, NumberOfFrames, FrameDurationMs, new Vector2(0, 0), rec1Scale, color);

                    float x = rec1.Right + (anim2.Bounds.Width / 2);
                    float y = rec2.Top - (anim2.Bounds.Height / 2);

                    anim2.Position = new Vector2(x, y);

                    rtnAnims.Add(anim2);
                }
            }
            else //Object 1 Top side collision
            {
                if (rec1.Left > rec2.Left)
                {
                    anim1.Load(SpriteSheetTextureSouthLeft, NumberOfFrames, FrameDurationMs, new Vector2(0, 0), rec1Scale, color);

                    float x = rec1.Left - (anim1.Bounds.Width / 2);
                    float y = rec2.Bottom + (anim1.Bounds.Height / 2);

                    anim1.Position = new Vector2(x, y);

                    rtnAnims.Add(anim1);
                }
                if (rec1.Right < rec2.Right)
                {
                    anim2.Load(SpriteSheetTextureSouthRight, NumberOfFrames, FrameDurationMs, new Vector2(0, 0), rec1Scale, color);

                    float x = rec1.Right + (anim2.Bounds.Width / 2);
                    float y = rec2.Bottom + (anim2.Bounds.Height / 2);

                    anim2.Position = new Vector2(x, y);

                    rtnAnims.Add(anim2);
                }
            }
        }


        return rtnAnims;
    }
}