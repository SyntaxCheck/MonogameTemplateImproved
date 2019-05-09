using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TextAnimatorPositionAndOpacity : TextAnimator
{
    public Color BaseColor { get; set; }
    public Vector2 CurrentPosition { get; set; }
    public Vector2 StartPosition { get; set; }
    public Vector2 EndPosition { get; set; }
    public Vector2 PositionDelta { get; set; }
    public int CurrentOpacity { get; set; }
    public int StartOpacity { get; set; }
    public int EndOpacity { get; set; }
    public int OpacityDelta { get; set; }
    public float CalculatedOpacity { get; set; }


    public TextAnimatorPositionAndOpacity()
    {
    }

    public void Load(string text, SpriteFont spriteFont, Texture2D backgroundTexture, double totalDurationMilliseconds, double millisecondsBetweenTicks, Vector2 startPosition, Vector2 endPosition, int startOpacity, int endOpacity, Color baseColor)
    {
        base.Load(text, spriteFont, backgroundTexture, totalDurationMilliseconds, millisecondsBetweenTicks);

        StartPosition = startPosition;
        EndPosition = endPosition;
        CurrentPosition = startPosition;
        StartOpacity = startOpacity;
        EndOpacity = endOpacity;
        CurrentOpacity = startOpacity;
        BaseColor = baseColor;
        CalculatedOpacity = startOpacity / 255;

        double tickCount = Math.Floor(TotalDurationMilliseconds / MillisecondsBetweenTicks);

        Vector2 totalPositionChange = Vector2.Subtract(EndPosition, StartPosition);
        PositionDelta = new Vector2(totalPositionChange.X / (float)tickCount, totalPositionChange.Y / (float)tickCount);

        int totalOpacityChange = EndOpacity - StartOpacity;
        OpacityDelta = (int)Math.Round(totalOpacityChange / tickCount, 0);
    }
    public override void Tick()
    {
        CurrentPosition += PositionDelta;
        CurrentOpacity += OpacityDelta;

        CalculatedOpacity = CurrentOpacity / (float)255;
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        if (BackgroundTexture != null)
            spriteBatch.Draw(BackgroundTexture, CurrentPosition, null, Color.GhostWhite * CalculatedOpacity, 0, BackgroundOrigin, 1f, SpriteEffects.None, 1);

        spriteBatch.DrawString(Font, Text, CurrentPosition, BaseColor * CalculatedOpacity, 0f, Origin, 1f, SpriteEffects.None, 1);
    }
}