using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

public enum Anchor
{
    BottomRight
}

public class MiniMap
{
    public bool PositionHasBeenSet { get; set; }
    public Texture2D WhitePixel { get; set; }
    public Texture2D BackgroundTexture { get; set; }
    public float BackgroundBorderWidth { get; set; }
    public Rectangle BackgroundTargetRectangle { get; set; }
    public Vector2 PositionCenter { get; set; }
    public Vector2 PositionUpperLeft { get; set; }
    public Vector2 Size { get; set; } //Size of the minimap excluding the border
    public Vector2 WorldSize { get; set; }
    public Anchor Anchor { get; set; }
    public Vector2 Ratio { get; set; }

    //Objects to be drawn
    public Rectangle ViewPort { get; set; }
    public Rectangle ViewPortLeft { get; set; }
    public Rectangle ViewPortRight { get; set; }
    public Rectangle ViewPortTop { get; set; }
    public Rectangle ViewPortBottom { get; set; }
    public List<MinimapObject> Buildings { get; set; }
    public List<MinimapObject> Characters { get; set; }

    public MiniMap(Texture2D whitePixel, Texture2D borderTexture, float backgroundBorderWidth)
    {
        WhitePixel = whitePixel;
        BackgroundTexture = borderTexture;
        BackgroundBorderWidth = backgroundBorderWidth;

        PositionHasBeenSet = false;
        Buildings = new List<MinimapObject>();
        Characters = new List<MinimapObject>();
    }

    public void SetPosition(GraphicsDevice graphics, Vector2 worldSize, Anchor anchor, Vector2 size)
    {
        WorldSize = worldSize;
        Anchor = anchor;
        Size = size;

        //This will hold the value we need to multiply the world coordinate by to find the minimap coordinate
        Ratio = new Vector2(Size.X / WorldSize.X, Size.Y / WorldSize.Y);

        int totalWidth = (int)(Math.Round(Size.X, 0) + (BackgroundBorderWidth * 2));
        int totalHeight = (int)(Math.Round(Size.Y, 0) + (BackgroundBorderWidth * 2));

        switch (Anchor)
        {
            case Anchor.BottomRight:
                PositionCenter = new Vector2(graphics.Viewport.Width - (totalWidth / 2), graphics.Viewport.Height - (totalHeight / 2));
                PositionUpperLeft = new Vector2(graphics.Viewport.Width - Size.X - BackgroundBorderWidth, graphics.Viewport.Height - Size.Y - BackgroundBorderWidth);

                break;
        }

        //Position X,Y in upper left
        BackgroundTargetRectangle = new Rectangle((int)Math.Round(PositionCenter.X, 0) - (totalWidth / 2), (int)Math.Round(PositionCenter.Y, 0) - (totalHeight / 2), totalWidth, totalHeight);
        PositionHasBeenSet = true;
    }
    public void UpdateMap(GameData gameData, Rectangle viewPortSize)
    {
        if (PositionHasBeenSet)
        {
            ViewPort = new Rectangle((int)(viewPortSize.X * Ratio.X + PositionUpperLeft.X), (int)(viewPortSize.Y * Ratio.Y + PositionUpperLeft.Y), (int)(viewPortSize.Width * Ratio.X), (int)(viewPortSize.Height * Ratio.Y));

            int viewPortThickness = 1;
            ViewPortLeft = new Rectangle(ViewPort.X - viewPortThickness, ViewPort.Y - viewPortThickness, viewPortThickness, ViewPort.Height + (viewPortThickness * 2));
            ViewPortRight = new Rectangle(ViewPort.Right + viewPortThickness, ViewPort.Y - viewPortThickness, viewPortThickness, ViewPort.Height + (viewPortThickness * 2));
            ViewPortTop = new Rectangle(ViewPort.X - viewPortThickness, ViewPort.Y - viewPortThickness, ViewPort.Width + (viewPortThickness * 2), viewPortThickness);
            ViewPortBottom = new Rectangle(ViewPort.X - viewPortThickness, ViewPort.Y + ViewPort.Height + viewPortThickness, ViewPort.Width + (viewPortThickness * 2), viewPortThickness);

            Characters.Clear();
            Buildings.Clear();

            for (int i = 0; i < gameData.Sprites.Count(); i++)
            {
                if (gameData.Sprites[i].MiniMapTexture != null)
                {
                    Vector2 miniMapPos = new Vector2(gameData.Sprites[i].Bounds.X, gameData.Sprites[i].Bounds.Y);
                    miniMapPos = miniMapPos * Ratio; //Scale the position down to the minimap size
                    miniMapPos += PositionUpperLeft; //Move the position to the correct location of the screen
                    miniMapPos -= new Vector2((gameData.Sprites[i].MiniMapTexture.Width * gameData.Sprites[i].MiniMapScale) / 2, (gameData.Sprites[i].MiniMapTexture.Height * gameData.Sprites[i].MiniMapScale) / 2); //Move the position to upperleft corner. The SpriteBase holds the position of the center

                    if (gameData.Sprites[i].IsMovable)
                    {
                        Characters.Add(new MinimapObject() { Texture = gameData.Sprites[i].MiniMapTexture, Color = gameData.Sprites[i].Color, Position = miniMapPos, Scale = gameData.Sprites[i].MiniMapScale, Opacity = gameData.Sprites[i].MiniMapOpacity });
                    }
                    else
                    {
                        Buildings.Add(new MinimapObject() { Texture = gameData.Sprites[i].MiniMapTexture, Color = gameData.Sprites[i].Color, Position = miniMapPos, Scale = gameData.Sprites[i].MiniMapScale, Opacity = gameData.Sprites[i].MiniMapOpacity });
                    }
                }
            }
        }
    }
    public void Draw(SpriteBatch spriteBatch)
    {
        if (PositionHasBeenSet)
        {
            spriteBatch.Draw(WhitePixel, BackgroundTargetRectangle, Color.White);
            spriteBatch.Draw(BackgroundTexture, BackgroundTargetRectangle, Color.Black);

            for (int i = 0; i < Buildings.Count(); i++)
            {
                spriteBatch.Draw(Buildings[i].Texture, Buildings[i].Position, null, Buildings[i].Color * Buildings[i].Opacity, 0f, Vector2.Zero, Buildings[i].Scale, SpriteEffects.None, 1f);
            }
            for (int i = 0; i < Characters.Count(); i++)
            {
                spriteBatch.Draw(Characters[i].Texture, Characters[i].Position, null, Characters[i].Color * Characters[i].Opacity, 0f, Vector2.Zero, Characters[i].Scale, SpriteEffects.None, 1f);
            }

            spriteBatch.Draw(WhitePixel, ViewPortLeft, Color.Red);
            spriteBatch.Draw(WhitePixel, ViewPortRight, Color.Red);
            spriteBatch.Draw(WhitePixel, ViewPortTop, Color.Red);
            spriteBatch.Draw(WhitePixel, ViewPortBottom, Color.Red);
        }
    }

    public Vector2 GetNewCameraPositionForPoint(Point mousePos)
    {
        Vector2 adjustedPosition = new Vector2(mousePos.X, mousePos.Y) - PositionUpperLeft;

        return adjustedPosition / Ratio;
    }
}