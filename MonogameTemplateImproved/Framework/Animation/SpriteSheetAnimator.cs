using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SpriteSheetAnimator : AnimationBase
{
    public Texture2D SheetFullTexture { get; set; }
    public Vector2 AdjustedSize { get; set; }
    public int TotalNumberOfFrameColumns { get; set; }
    public int FrameWidth { get; set; }
    public int FrameHeight { get; set; }
    public int StartFrame { get; set; }
    public int EndFrame { get; set; }
    public int CurrentFrame { get; set; }
    public double FrameDurationMilliseconds { get; set; }
    public double ElapsedMillisecondsSinceFrameChange { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Origin { get; set; }
    public float Scale { get; set; }
    public Rectangle Bounds { get; set; }
    public Color FillColor { get; set; }
    public Rectangle FrameRectangle { get; set; }

    public SpriteSheetAnimator()
    {
        IsAnimationComplete = false;
        ElapsedMillisecondsSinceFrameChange = 0;
    }

    public void Load(Texture2D texture, int totalNumberOfFrameColumns, double frameDurationMilliseconds, Vector2 position, Color fillColor)
    {
        Load(texture, totalNumberOfFrameColumns, 0, totalNumberOfFrameColumns - 1, frameDurationMilliseconds, position, fillColor);
    }
    public void Load(Texture2D texture, int totalNumberOfFrameColumns, double frameDurationMilliseconds, Vector2 position, float scale, Color fillColor)
    {
        Load(texture, totalNumberOfFrameColumns, 0, totalNumberOfFrameColumns - 1, frameDurationMilliseconds, position, scale, fillColor);
    }
    public void Load(Texture2D texture, int totalNumberOfFrameColumns, int startingFrame, int endingFrame, double frameDurationMilliseconds, Vector2 position, Color fillColor)
    {
        Load(texture, totalNumberOfFrameColumns, startingFrame, endingFrame, frameDurationMilliseconds, position, 1f, fillColor);
    }
    public void Load(Texture2D texture, int totalNumberOfFrameColumns, int startingFrame, int endingFrame, double frameDurationMilliseconds, Vector2 position, float scale, Color fillColor)
    {
        SheetFullTexture = texture;
        TotalNumberOfFrameColumns = totalNumberOfFrameColumns;
        FrameWidth = texture.Width / totalNumberOfFrameColumns;
        FrameHeight = texture.Height;
        StartFrame = startingFrame;
        EndFrame = endingFrame;
        CurrentFrame = startingFrame;
        FrameDurationMilliseconds = frameDurationMilliseconds;
        Position = position;
        FillColor = fillColor;
        Scale = scale;
        AdjustedSize = new Vector2(SheetFullTexture.Width * Scale, SheetFullTexture.Height * Scale); //Get the Adjusted size based on the scale of the object
        int AdjustedFrameWidth = (int)AdjustedSize.X / totalNumberOfFrameColumns;
        int AdjustedFrameHeight = (int)AdjustedSize.Y;
        Origin = new Vector2(FrameWidth / 2, FrameHeight / 2);
        Bounds = new Rectangle((int)(Position.X - (AdjustedFrameWidth / 2)), (int)(Position.Y - (AdjustedFrameHeight / 2)), AdjustedFrameWidth, AdjustedFrameHeight); //Bounds will used the Adjusted value
        FrameRectangle = new Rectangle(CurrentFrame * FrameWidth, 0, FrameWidth, FrameHeight); //FrameRectangle works off the base SpriteSheet image so it needs to use the non-scaled values
    }
    public override void Update(GameTime gameTime)
    {
        if (!IsAnimationComplete)
        {
            ElapsedMillisecondsSinceFrameChange += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (ElapsedMillisecondsSinceFrameChange >= FrameDurationMilliseconds)
            {
                if (CurrentFrame < EndFrame)
                {
                    ElapsedMillisecondsSinceFrameChange -= FrameDurationMilliseconds; //Dont reset to 0, if we over shot it then the next frame will just lose time
                    CurrentFrame++;
                    FrameRectangle = new Rectangle(CurrentFrame * FrameWidth, 0, FrameWidth, FrameHeight);
                }
                else
                {
                    IsAnimationComplete = true;
                }
            }
        }

        base.Update(gameTime);
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(SheetFullTexture, Position, FrameRectangle, FillColor, 0, Origin, Scale, SpriteEffects.None, 1);
    }
}