using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TextAnimator : AnimationBase
{
    public Texture2D BackgroundTexture { get; set; }
    public string Text { get; set; }
    public Vector2 FontSize { get; set; }
    public Vector2 Origin { get; set; }
    public Vector2 BackgroundOrigin { get; set; }
    public SpriteFont Font { get; set; }
    public double TotalDurationMilliseconds { get; set; }
    public double TotalElapsedMilliseconds { get; set; }
    public double MillisecondsBetweenTicks { get; set; }
    public double ElapsedMillisecondsSinceTick { get; set; }

    public TextAnimator()
    {
        IsAnimationComplete = false;
        TotalElapsedMilliseconds = 0;
        ElapsedMillisecondsSinceTick = 0;
    }

    public virtual void Load(string text, SpriteFont spriteFont, Texture2D backgroundTexture, double totalDurationMilliseconds, double millisecondsBetweenTicks)
    {
        Text = text;
        Font = spriteFont;
        BackgroundTexture = backgroundTexture;
        TotalDurationMilliseconds = totalDurationMilliseconds;
        MillisecondsBetweenTicks = millisecondsBetweenTicks;
        FontSize = Font.MeasureString(text);
        Origin = new Vector2(FontSize.X / 2f, FontSize.Y / 2f);
        if(backgroundTexture != null)
            BackgroundOrigin = new Vector2(backgroundTexture.Width / 2f, backgroundTexture.Height / 2f);
    }
    public override void Update(GameTime gameTime)
    {
        if (!IsAnimationComplete)
        {
            TotalElapsedMilliseconds += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (TotalElapsedMilliseconds >= TotalDurationMilliseconds)
            {
                IsAnimationComplete = true;
            }
            else
            {
                ElapsedMillisecondsSinceTick += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (ElapsedMillisecondsSinceTick >= MillisecondsBetweenTicks)
                {
                    ElapsedMillisecondsSinceTick -= MillisecondsBetweenTicks;
                    Tick();
                }
            }
        }

        base.Update(gameTime);
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
    }
    public virtual void Tick()
    {
    }
}