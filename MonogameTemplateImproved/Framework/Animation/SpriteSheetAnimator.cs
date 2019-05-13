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
    public int FrameMoveAmount { get; set; } //Can be negative or positive
    public int CurrentFrame { get; set; }
    public int CurrentRow { get; set; }
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

    public void Load(Texture2D texture, double totalAnimationDuration, int totalNumberOfFrameColumns, int totalNumberOfFrameRows, int frameRow, Vector2 position, Color fillColor)
    {
        Load(texture, totalNumberOfFrameColumns, totalNumberOfFrameRows, frameRow, totalAnimationDuration / totalNumberOfFrameColumns, position, fillColor);
    }
    public void Load(Texture2D texture, int totalNumberOfFrameColumns, int totalNumberOfFrameRows, int frameRow, double frameDurationMilliseconds, Vector2 position, Color fillColor)
    {
        Load(texture, totalNumberOfFrameColumns, totalNumberOfFrameRows, frameRow, 0, totalNumberOfFrameColumns - 1, frameDurationMilliseconds, position, fillColor);
    }
    public void Load(Texture2D texture, int totalNumberOfFrameColumns, int totalNumberOfFrameRows, int frameRow, double frameDurationMilliseconds, Vector2 position, float scale, Color fillColor)
    {
        Load(texture, totalNumberOfFrameColumns, totalNumberOfFrameRows, frameRow, 0, totalNumberOfFrameColumns - 1, 1, frameDurationMilliseconds, position, scale, fillColor);
    }
    public void Load(Texture2D texture, int totalNumberOfFrameColumns, int totalNumberOfFrameRows, int frameRow, int startingFrame, int endingFrame, double frameDurationMilliseconds, Vector2 position, Color fillColor)
    {
        Load(texture, totalNumberOfFrameColumns, totalNumberOfFrameRows, frameRow, startingFrame, endingFrame, 1, frameDurationMilliseconds, position, 1f, fillColor);
    }
    public void Load(Texture2D texture, int totalNumberOfFrameColumns, int totalNumberOfFrameRows, int frameRow, int startingFrame, int endingFrame, int frameMoveAmount, double frameDurationMilliseconds, Vector2 position, Color fillColor)
    {
        Load(texture, totalNumberOfFrameColumns, totalNumberOfFrameRows, frameRow, startingFrame, endingFrame, frameMoveAmount, frameDurationMilliseconds, position, 1f, fillColor);
    }
    public void Load(Texture2D texture, int totalNumberOfFrameColumns, int totalNumberOfFrameRows, int frameRow, int startingFrame, int endingFrame, int frameMoveAmount, double frameDurationMilliseconds, Vector2 position, float scale, Color fillColor)
    {
        SheetFullTexture = texture;
        TotalNumberOfFrameColumns = totalNumberOfFrameColumns;
        FrameWidth = texture.Width / totalNumberOfFrameColumns;
        FrameHeight = texture.Height / totalNumberOfFrameRows;
        StartFrame = startingFrame;
        EndFrame = endingFrame;
        FrameMoveAmount = frameMoveAmount;
        CurrentFrame = startingFrame;
        CurrentRow = frameRow;
        FrameDurationMilliseconds = frameDurationMilliseconds;
        Position = position;
        FillColor = fillColor;
        Scale = scale;
        AdjustedSize = new Vector2(SheetFullTexture.Width * Scale, SheetFullTexture.Height * Scale); //Get the Adjusted size based on the scale of the object
        int AdjustedFrameWidth = (int)AdjustedSize.X / totalNumberOfFrameColumns;
        int AdjustedFrameHeight = (int)AdjustedSize.Y / totalNumberOfFrameRows;
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
                if ((FrameMoveAmount > 0 && CurrentFrame < EndFrame) || (FrameMoveAmount < 0 && CurrentFrame > 0))
                {
                    ElapsedMillisecondsSinceFrameChange -= FrameDurationMilliseconds; //Dont reset to 0, if we over shot it then the next frame will just lose time
                    CurrentFrame += FrameMoveAmount;

                    //Make sure we dont jump outside of the spritesheet. Adjust the position instead of ending the animation so that the final frame can be played
                    if (CurrentFrame > EndFrame)
                        CurrentFrame = EndFrame;
                    else if (CurrentFrame < 0)
                        CurrentFrame = 0;

                    FrameRectangle = new Rectangle(CurrentFrame * FrameWidth, CurrentRow * FrameHeight, FrameWidth, FrameHeight);
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
        if(!IsAnimationComplete)
            spriteBatch.Draw(SheetFullTexture, Position, FrameRectangle, FillColor, 0, Origin, Scale, SpriteEffects.None, 1);
    }
}