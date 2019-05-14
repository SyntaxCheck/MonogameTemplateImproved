using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RectangleFLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class SpriteBase
{
    public abstract bool IsMovable { get; set; }

    private RectangleF _bounds;
    private Texture2D _texture;
    private Vector2 _position;
    private List<Point> _gridPositions;
    private float _rotation;
    private float _scale;

    public List<Point> GridPositions
    {
        get
        {
            return _gridPositions;
        }
        set
        {
            OldGridPositions = _gridPositions;
            _gridPositions = value;
        }
    } //The list of grid positions
    public List<Point> OldGridPositions { get; set; } //The list of grid positions
    public Texture2D Texture
    {
        get
        {
            return _texture;
        }
        set
        {
            _texture = value;
            AdjustedSize = new Vector2(_texture.Width * _scale, _texture.Height * _scale);
            Origin = new Vector2(_texture.Width / 2, _texture.Height / 2);
            TextureCollideDistance = (int)Math.Ceiling(Math.Sqrt(AdjustedSize.X * AdjustedSize.X + AdjustedSize.Y * AdjustedSize.Y));
            CalculateBounds();
        }
    }
    public Vector2 Position
    {
        get
        {
            return _position;
        }
        set
        {
            _position = value;

            if (Texture != null)
            {
                _bounds.X = _position.X - (_bounds.Width / 2);
                _bounds.Y = _position.Y - (_bounds.Height / 2);
            }

            if (_bounds.Left < 0)
            {
                _position.X = _bounds.Width / 2;
                _bounds.X = 0;
            }
            if (_bounds.Top < 0)
            {
                _position.Y = _bounds.Height / 2;
                _bounds.Y = 0;
            }
            if (_bounds.Right > WorldSize)
            {
                _position.X = WorldSize - (_bounds.Width / 2);
                _bounds.X = WorldSize - _bounds.Width;
            }
            if (_bounds.Bottom > WorldSize)
            {
                _position.Y = WorldSize - (_bounds.Height / 2);
                _bounds.Y = WorldSize - _bounds.Height;
            }
        }
    }
    public Vector2 AdjustedSize { get; set; }
    public Vector2 Direction { get; set; }
    public Vector2 Origin { get; set; }
    public RectangleF Bounds
    {
        get
        {
            return _bounds;
        }
        set
        {
            _bounds = value;
        }
    }
    public Texture2D MiniMapTexture { get; set; }
    public float MiniMapScale { get; set; }
    public float MiniMapOpacity { get; set; }
    public int WorldSize { get; set; }
    public int TextureCollideDistance { get; set; }
    public string CurrentGridPositionsForCompare { get; set; }
    public string OldGridPositionsForCompare { get; set; }
    public float Rotation
    {
        get { return _rotation; }
        set
        {
            _rotation = value;
            Direction = new Vector2((float)Math.Cos(Rotation - MathHelper.ToRadians(90)), (float)Math.Sin(Rotation - MathHelper.ToRadians(90)));
            Direction.Normalize();
        }
    }
    public float Scale
    {
        get
        {
            return _scale;
        }
        set
        {
            _scale = value;
            AdjustedSize = new Vector2(_texture.Width * _scale, _texture.Height * _scale);
            TextureCollideDistance = (int)Math.Ceiling(Math.Sqrt(AdjustedSize.X * AdjustedSize.X + AdjustedSize.Y * AdjustedSize.Y));
            CalculateBounds();
        }
    }
    public float Speed { get; set; }
    public float ScreenDepth { get; set; }
    public Color Color { get; set; }
    public float Opacity { get; set; }
    public bool IsAlive { get; set; }
    public bool DrawObject { get; set; } //Is the object on screen and should we draw it
    //Debug Properties
    public Texture2D WhiteTexture { get; set; }
    public SpriteFont DebugFont { get; set; }

    public SpriteBase()
    {
        _position = Vector2.Zero;
        _scale = 1f; //Default scale to full size
        MiniMapScale = 1f; //Default scale to full size
        MiniMapOpacity = 1f; //Default to fully visible
        Opacity = 1f; //Default to fully visible
        GridPositions = new List<Point>();
        OldGridPositions = new List<Point>();
        CurrentGridPositionsForCompare = String.Empty;
        OldGridPositionsForCompare = String.Empty;
    }

    public void CalculateBounds()
    {
        Bounds = new RectangleF((Position.X - (AdjustedSize.X / 2)), (Position.Y - (AdjustedSize.Y / 2)), AdjustedSize.X, AdjustedSize.Y);
    }
    public Vector2 CalculateGridPositionVector(int cellSize)
    {
        Vector2 pos = new Vector2();

        //Divide the position by the cell size then cast to int which does the same as Math.Floor. 
        //Multiply by the cell size to find the upper left cell corner. 
        //This only tells you the cell the object is in centered over not accounting for texture height/width
        pos.X = (int)(Position.X / cellSize) * cellSize;
        pos.Y = (int)(Position.Y / cellSize) * cellSize;

        return pos;
    }
    public Point CalculateGridPosition(int cellSize)
    {
        Point pos = new Point();
        int maxIndex = WorldSize / cellSize - 1;
        //Divide the position by the cell size then cast to int which does the same as Math.Floor. 
        //Multiply by the cell size to find the upper left cell corner. 
        //This only tells you the cell the object is in centered over not accounting for texture height/width
        pos.X = (int)(Position.X / cellSize);
        pos.Y = (int)(Position.Y / cellSize);

        if (pos.X < 0)
            pos.X = 0;
        if (pos.Y < 0)
            pos.Y = 0;
        if (pos.X > maxIndex)
            pos.X = maxIndex;
        if (pos.Y > maxIndex)
            pos.Y = maxIndex;

        return pos;
    }
    public List<Point> GetGridDeltaAdd()
    {
        List<Point> delta = new List<Point>();

        foreach (Point p in GridPositions)
        {
            bool found = false;

            foreach (Point o in OldGridPositions)
            {
                if (p.X == o.X && p.Y == o.Y)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                delta.Add(p);
            }
        }

        return delta;
    }
    public List<Point> GetGridDelta()
    {
        List<Point> delta = new List<Point>();

        foreach (Point p in OldGridPositions)
        {
            bool found = false;

            foreach (Point o in GridPositions)
            {
                if (p.X == o.X && p.Y == o.Y)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                delta.Add(p);
            }
        }

        return delta;
    }
    public void GetGridPositionsForSpriteBase(GameData _gameData)
    {
        List<Point> gridPositions = new List<Point>();

        //Find the top left grid position and the bottom right grid position
        Point topLeft = new Point((int)Math.Floor(Bounds.X / _gameData.Settings.GridCellSize), (int)Math.Floor(Bounds.Y / _gameData.Settings.GridCellSize));
        Point bottomRight = new Point((int)Math.Floor((Bounds.X + Bounds.Width) / _gameData.Settings.GridCellSize), (int)Math.Floor((Bounds.Y + Bounds.Height) / _gameData.Settings.GridCellSize));

        for(int y = topLeft.Y; y <= bottomRight.Y; y++)
        {
            for(int x = topLeft.X; x <= bottomRight.X; x++)
            {
                if(x >= 0 && x < _gameData.MapGridData.GetLength(0) && y >= 0 && y < _gameData.MapGridData.GetLength(1))
                    gridPositions.Add(new Point(x, y));
            }
        }

        //Move the Current Grid position string to the Old
        string gridPosition = String.Empty;
        foreach (Point p in gridPositions)
        {
            //gridPosition += p.X + "," + p.Y + " ";
            gridPosition = String.Join(",", new String[] { p.X.ToString(), p.Y.ToString(), " " }); //String Join for Efficiency 
        }

        OldGridPositionsForCompare = CurrentGridPositionsForCompare;
        CurrentGridPositionsForCompare = gridPosition;

        GridPositions = gridPositions;
    }

    public virtual void Update(GameTime gameTime, ref GameData _gameData)
    {
        if (IsAlive && IsMovable)
        {
            Position += Direction * (Speed * (float)gameTime.ElapsedGameTime.TotalSeconds);

            GetGridPositionsForSpriteBase(_gameData);

            if (CurrentGridPositionsForCompare != OldGridPositionsForCompare)
            {
                //Remove delta
                List<Point> delta = GetGridDelta();
                if (delta.Count > 0)
                {
                    _gameData.RemoveSpriteFromGrid(this, delta);
                }

                //Add delta
                delta = GetGridDeltaAdd();
                if (delta.Count > 0)
                {
                    _gameData.AddSpriteDeltaToGrid(this, delta);
                }
            }
        }
    }
    public virtual void Draw(SpriteBatch _spriteBatch)
    {
        if(IsAlive && DrawObject)
            _spriteBatch.Draw(Texture, Position, null, Color * Opacity, Rotation, Origin, Scale, SpriteEffects.None, ScreenDepth);
    }
    public void DrawDebugOutlineForSprite(SpriteBatch _spriteBatch)
    {
        int borderWidth = 3;
        int diagnolLength = (int)Math.Ceiling(Math.Sqrt((AdjustedSize.X * AdjustedSize.X) + (AdjustedSize.Y * AdjustedSize.Y)));
        float upperLeftX = Position.X - (diagnolLength / 2), upperLeftY = Position.Y - (diagnolLength / 2);
        float upperLeftReg = Position.X - (Bounds.Width / 2), upperLeftYReg = Position.Y - (Bounds.Width / 2);

        _spriteBatch.Draw(WhiteTexture, new Rectangle((int)upperLeftX - borderWidth, (int)upperLeftY - borderWidth, diagnolLength + borderWidth * 2, borderWidth), Color.Red);
        _spriteBatch.Draw(WhiteTexture, new Rectangle((int)upperLeftX - borderWidth, (int)upperLeftY + diagnolLength, diagnolLength + borderWidth * 2, borderWidth), Color.Red);
        _spriteBatch.Draw(WhiteTexture, new Rectangle((int)upperLeftX + diagnolLength, (int)upperLeftY - borderWidth, borderWidth, diagnolLength + borderWidth * 2), Color.Red);
        _spriteBatch.Draw(WhiteTexture, new Rectangle((int)upperLeftX - borderWidth, (int)upperLeftY - borderWidth, borderWidth, diagnolLength + borderWidth * 2), Color.Red);

        _spriteBatch.Draw(WhiteTexture, new Rectangle((int)upperLeftReg - borderWidth, (int)upperLeftYReg - borderWidth, (int)Bounds.Width + borderWidth * 2, borderWidth), Color.Red);
        _spriteBatch.Draw(WhiteTexture, new Rectangle((int)upperLeftReg - borderWidth, (int)upperLeftYReg + (int)Bounds.Width, (int)Bounds.Width + borderWidth * 2, borderWidth), Color.Red);
        _spriteBatch.Draw(WhiteTexture, new Rectangle((int)upperLeftReg + (int)Bounds.Width, (int)upperLeftYReg - borderWidth, borderWidth, (int)Bounds.Width + borderWidth * 2), Color.Red);
        _spriteBatch.Draw(WhiteTexture, new Rectangle((int)upperLeftReg - borderWidth, (int)upperLeftYReg - borderWidth, borderWidth, (int)Bounds.Width + borderWidth * 2), Color.Red);
    }
    public void DrawDebugDataForSprite(SpriteBatch _spriteBatch, bool left)
    {
        List<string> debugInfo = new List<string>();
        debugInfo.Add("Position: " + (int)Position.X + "," + (int)Position.Y);
        debugInfo.Add("Direction: " + (int)Direction.X + "," + (int)Direction.Y);
        debugInfo.Add("Rotation R: " + Math.Round(Rotation, 2));
        debugInfo.Add("Rotation D: " + Math.Round(MathHelper.ToDegrees(Rotation), 2));

        foreach (Point p in GridPositions)
        {
            debugInfo.Add("MapCell: " + p.X + "," + p.Y);
        }
        //for (int x = 0; x < _gameData.MapGridData.GetLength(0); x++)
        //{
        //    for (int y = 0; y < _gameData.MapGridData.GetLength(1); y++)
        //    {
        //        if (_gameData.MapGridData[x, y].Creatures.Contains(creature))
        //        {
        //            debugInfo.Add("MapCell: " + x + "," + y);
        //        }
        //    }
        //}

        int lockWidth = 125;
        if (left)
            DrawDebugPanel(_spriteBatch, debugInfo, lockWidth, new Vector2(Bounds.Left, Bounds.Bottom + 10));
        else
            DrawDebugPanel(_spriteBatch, debugInfo, lockWidth, new Vector2(Bounds.Left, Bounds.Bottom + 10));

        //if (left)
        //    DrawDebugPanel(_spriteBatch, debugInfo, lockWidth, new Vector2(Position.X - (Texture.Width / 2) - 5 - lockWidth, Position.Y - (Texture.Height / 2)));
        //else
        //    DrawDebugPanel(_spriteBatch, debugInfo, lockWidth, new Vector2(Position.X + (Texture.Width / 2) + 5, Position.Y - (Texture.Height / 2)));
    }
    private void DrawDebugPanel(SpriteBatch _spriteBatch, List<string> text, int lockedWidth, Vector2 position)
    {
        int width = lockedWidth;
        int height = 0;
        int textHeight = 0;
        int textSpacing = 5;
        int borderDepth = 2;
        int startingX = (int)Math.Ceiling(position.X);
        int startingY = (int)Math.Ceiling(position.Y);
        int currentX, currentY;

        if (lockedWidth == 0) //Calculate the Width if lock width not specified
        {
            int maxWidth = 0;

            for (int i = 0; i < text.Count; i++)
            {
                Vector2 size = DebugFont.MeasureString(text[i]);
                textHeight = (int)Math.Ceiling(size.Y);
                int tmpWidth = (int)Math.Ceiling(size.X);
                if (tmpWidth > maxWidth)
                    maxWidth = tmpWidth;
            }

            width = maxWidth + (textSpacing * 2) + (borderDepth * 2);
        }
        else
        {
            Vector2 size = DebugFont.MeasureString("AGHIQZXVY[]qyp");
            textHeight = (int)Math.Ceiling(size.Y);
        }

        height = text.Count * (textHeight + textSpacing) + textSpacing;

        //Draw the Background border
        _spriteBatch.Draw(WhiteTexture, new Rectangle(startingX, startingY, width, height), Color.Black);
        _spriteBatch.Draw(WhiteTexture, new Rectangle(startingX + borderDepth, startingY + borderDepth, width - borderDepth * 2, height - borderDepth * 2), Color.White);

        currentX = startingX + borderDepth + textSpacing;
        currentY = startingY + borderDepth + textSpacing;

        for (int i = 0; i < text.Count; i++)
        {
            _spriteBatch.DrawString(DebugFont, text[i], new Vector2(currentX, currentY), Color.Black);
            currentY += textHeight + textSpacing;
        }
    }
}