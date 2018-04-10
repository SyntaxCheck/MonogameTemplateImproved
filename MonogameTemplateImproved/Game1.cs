using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

/// <summary>
/// This is the main type for your game.
/// </summary>
public class Game1 : Game
{
    //Framework variables
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private InputState _inputState;
    private Player _player;
    private SpriteFont _diagFont;
    //Game variables
    private GameData _gameData;
    private Borders _borders;
    private Random _rand;
    private Texture2D _whitePixel;
    private int _frames;
    private double _elapsedSeconds;
    private int _fps;
    private double _elapsedSecondsSinceTick;
    private double _elapsedTimeSinceFoodGeneration;
    private float _currentTicksPerSecond = 30;
    private float _tickSeconds;
    private float _elapsedTicksSinceSecondProcessing;
    //Constants
    private const int GRID_CELL_SIZE = 50; //Seems to be the sweet spot for a 5,000 x 5,000 map based on the texture sizes we have so far
    private const int BORDER_WIDTH = 10;
    private const float TICKS_PER_SECOND = 30;
    //Colors
    private Color MAP_COLOR = Color.AliceBlue;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferHeight = 900;
        _graphics.PreferredBackBufferWidth = 1600;

        IsMouseVisible = true;

        Content.RootDirectory = "Content";
    }

    /// <summary>
    /// Allows the game to perform any initialization it needs to before starting to run.
    /// This is where it can query for any required services and load any non-graphic
    /// related content.  Calling base.Initialize will enumerate through any components
    /// and initialize them as well.
    /// </summary>
    protected override void Initialize()
    {
        Global.Camera.ViewportWidth = _graphics.GraphicsDevice.Viewport.Width;
        Global.Camera.ViewportHeight = _graphics.GraphicsDevice.Viewport.Height;
        Global.Camera.CenterOn(new Vector2(Global.Camera.ViewportWidth / 2, Global.Camera.ViewportHeight / 2));

        base.Initialize();
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
        //Load settings at the beginning
        _gameData = new GameData();
        _gameData.Settings = SettingsHelper.ReadSettings("Settings.json");

        //Init variables
        InitVariables();

        // Create a new SpriteBatch, which can be used to draw textures.
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        //Load the Font from the Content object. Use the Content Pipeline under the  "Content" folder to add assets to game
        _diagFont = Content.Load<SpriteFont>("DiagnosticsFont");

        //Load in a simple white pixel
        _whitePixel = new Texture2D(_graphics.GraphicsDevice, 1, 1);
        Color[] color = new Color[1];
        color[0] = Color.White;
        _whitePixel.SetData(color);

        _rand = new Random();

        //Generate the Map
        _borders = new Borders();
        _borders.Texture = _whitePixel;
        _borders.LeftWall = new Vector2(0, 0);
        _borders.RightWall = new Vector2(_gameData.Settings.WorldSize, 0);
        _borders.TopWall = new Vector2(0, 0);
        _borders.BottomWall = new Vector2(0, _gameData.Settings.WorldSize);

        //Initialize the Grid
        int gridWidth = (int)Math.Ceiling((double)_gameData.Settings.WorldSize / GRID_CELL_SIZE);

        _gameData.MapGridData = new GridData[gridWidth, gridWidth];

        //Loop through grid and set Rectangle on each cell, named iterators x,y to help avoid confusion
        for (int y = 0; y < _gameData.MapGridData.GetLength(0); y++)
        {
            for (int x = 0; x < _gameData.MapGridData.GetLength(1); x++)
            {
                _gameData.MapGridData[x, y] = new GridData();
                _gameData.MapGridData[x, y].Sprites = new List<SpriteBase>();

                Rectangle rec = new Rectangle();
                rec.X = x * GRID_CELL_SIZE;
                rec.Y = y * GRID_CELL_SIZE;
                rec.Width = GRID_CELL_SIZE;
                rec.Height = GRID_CELL_SIZE;

                _gameData.MapGridData[x, y].CellRectangle = rec;
            }
        }

        for (int i = 0; i < 50; i++)
        {
            SpawnTestObject();
        }
    }

    /// <summary>
    /// UnloadContent will be called once per game and is the place to unload
    /// game-specific content.
    /// </summary>
    protected override void UnloadContent()
    {
        // TODO: Unload any non ContentManager content here
    }

    /// <summary>
    /// Allows the game to run logic such as updating the world,
    /// checking for collisions, gathering input, and playing audio.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Update(GameTime gameTime)
    {
        bool tick = false;

        if (_inputState.IsExitGame(PlayerIndex.One))
        {
            Exit();
        }
        else
        {
            _inputState.Update();
            _player.HandleInput(_inputState);
            Global.Camera.HandleInput(_inputState, PlayerIndex.One, ref _gameData);

            _tickSeconds = 1 / _currentTicksPerSecond;

            _elapsedSecondsSinceTick += gameTime.ElapsedGameTime.TotalSeconds;
            _elapsedTimeSinceFoodGeneration += gameTime.ElapsedGameTime.TotalSeconds;
            if (_elapsedSecondsSinceTick > _tickSeconds)
            {
                _elapsedSecondsSinceTick = _elapsedSecondsSinceTick - _tickSeconds; //Start the next tick with the overage
                tick = true;
            }

            //During a tick do all creature processing
            if (tick)
            {
                UpdateTick(gameTime);
            }
            else //Off tick processing
            {
                UpdateOffTick(gameTime);
            }
        }

        //This must be after movement caluclations occur for the creatures otherwise the camera will glitch back and forth
        if (_gameData.Focus != null)
        {
            Global.Camera.CenterOn(_gameData.Focus.Position);
        }

        base.Update(gameTime);
    }

    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
        //FPS Logic
        if (_elapsedSeconds >= 1)
        {
            _fps = _frames;
            _frames = 0;
            _elapsedSeconds = 0;
        }
        _frames++;
        _elapsedSeconds += gameTime.ElapsedGameTime.TotalSeconds;

        GraphicsDevice.Clear(MAP_COLOR);

        //DRAW IN THE WORLD
        _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, Global.Camera.TranslationMatrix);
        ////Sample Draws to test Panning/Zooming. Notice that choosing a color on the Draw Results in the pixel color changing. This works because putting a tint onto white results in that color 
        //_spriteBatch.Draw(_whitePixel, new Rectangle(Global.Camera.ViewportWidth / 2, Global.Camera.ViewportHeight / 2, 10, 10), Color.Black);
        //_spriteBatch.Draw(_whitePixel, new Rectangle(-100, 100, 10, 10), Color.Red);

        for (int i = 0; i < _gameData.Sprites.Count; i++)
        {
            _spriteBatch.Draw(_gameData.Sprites[i].Texture, _gameData.Sprites[i].Position, null, Color.White, _gameData.Sprites[i].Rotation, _gameData.Sprites[i].Origin, 1f, SpriteEffects.None, 1f);
        }
        DrawBorders();
        _spriteBatch.End();

        //DRAW HUD INFOMATION
        _spriteBatch.Begin();
        //FPS Counter in top left corner
        _spriteBatch.DrawString(_diagFont, "FPS: " + _fps, new Vector2(10, 10), Color.Black);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    //Private functions
    private void InitVariables()
    {
        _inputState = new InputState();
        _player = new Player();
        _fps = 0;
        _frames = 0;
        _elapsedSeconds = 0.0;
    }
    private void SpawnTestObject()
    {
        Car sprite = new Car();
        sprite.IsAlive = true;
        sprite.WorldSize = _gameData.Settings.WorldSize;
        sprite.Texture = BuildSampleImage(_graphics.GraphicsDevice);
        sprite.Speed = 50f;
        sprite.Rotation = MathHelper.ToRadians(_rand.Next(0, 360));
        sprite.Position = new Vector2(_rand.Next(sprite.Texture.Width, _gameData.Settings.WorldSize - sprite.Texture.Width), _rand.Next(sprite.Texture.Height, _gameData.Settings.WorldSize - sprite.Texture.Height));
        sprite.GetGridPositionsForSpriteBase(GRID_CELL_SIZE, _gameData);

        _gameData.Sprites.Add(sprite);
        _gameData.AddSpriteToGrid(sprite);
    }
    private Texture2D BuildSampleImage(GraphicsDevice device)
    {
        int IMAGE_WIDTH = 32;
        int IMAGE_HEIGHT = 32;
        Texture2D texture = new Texture2D(device, IMAGE_WIDTH, IMAGE_HEIGHT);

        List<Color> pixels = new List<Color>();
        for (int i = 0; i < (IMAGE_WIDTH * IMAGE_HEIGHT); i++)
        {
            pixels.Add(Color.Red);
        }

        texture.SetData(pixels.ToArray());

        return texture;
    }

    //Update functions
    private void UpdateTick(GameTime gameTime)
    {
        _elapsedTicksSinceSecondProcessing++;

        UpdateTickSprites(gameTime);
    }
    private void UpdateTickSprites(GameTime gameTime)
    {
        for (int i = _gameData.Sprites.Count - 1; i >= 0; i--)
        {
            UpdateMoveSprite(gameTime, i);
        }
    }
    private void UpdateOffTick(GameTime gameTime)
    {
        //Collisions And Movement
        UpdateOffTickHandleCollisionsAndMovement(gameTime);

        //Every second interval processing only when it is not a TICK. When things only need to be updated once every X seconds
        if (_elapsedTicksSinceSecondProcessing >= TICKS_PER_SECOND * 5)
        {
            UpdateOffTickInterval(gameTime);
        }
    }
    private void UpdateOffTickInterval(GameTime gameTime)
    {
        _elapsedTicksSinceSecondProcessing = 0;
    }
    private void UpdateOffTickHandleCollisionsAndMovement(GameTime gameTime)
    {
        List<SpriteBase> deadSpritesToRemove = new List<SpriteBase>();

        //CollisionDetection
        //Border Collision Detection
        for (int i = 0; i < _gameData.Sprites.Count; i++)
        {
            if (_gameData.Sprites[i].IsAlive)
            {
                //Change rotation on wall collision
                if (_gameData.Sprites[i].Position.X - (_gameData.Sprites[i].Texture.Width / 2) <= 0 || _gameData.Sprites[i].Position.X + (_gameData.Sprites[i].Texture.Width / 2) >= _gameData.Settings.WorldSize)
                {
                    if (_gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                        _gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y < 0 ||
                        _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                        _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y < 0)
                    {
                        _gameData.Sprites[i].Rotation = (((float)Math.PI * 2) - _gameData.Sprites[i].Rotation);
                    }
                }
                if (_gameData.Sprites[i].Position.Y - (_gameData.Sprites[i].Texture.Height / 2) <= 0 || _gameData.Sprites[i].Position.Y + (_gameData.Sprites[i].Texture.Height / 2) >= _gameData.Settings.WorldSize)
                {
                    if (_gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                        _gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y < 0 ||
                        _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                        _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y < 0)
                    {
                        _gameData.Sprites[i].Rotation = (((float)Math.PI) - _gameData.Sprites[i].Rotation);
                    }
                }

                ////Collision
                //foreach (Point p in _gameData.Sprites[i].GridPositions)
                //{
                //    for (int k = (_gameData.MapGridData[p.X, p.Y].Sprites.Count - 1); k >= 0; k--)
                //    {
                //        if (_gameData.Sprites[i] != _gameData.Sprites[k] && _gameData.Sprites[i].Bounds.Intersects(_gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds))
                //        {
                //            //Change rotation on object collision just for a sample
                //            if (_gameData.Sprites[i].Position.X - (_gameData.Sprites[i].Texture.Width / 2) <= 0 || _gameData.Sprites[i].Position.X + (_gameData.Sprites[i].Texture.Width / 2) >= _gameData.Settings.WorldSize)
                //            {
                //                if (_gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                //                    _gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y < 0 ||
                //                    _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                //                    _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y < 0)
                //                {
                //                    _gameData.Sprites[i].Rotation = (((float)Math.PI * 2) - _gameData.Sprites[i].Rotation);
                //                }
                //            }
                //            if (_gameData.Sprites[i].Position.Y - (_gameData.Sprites[i].Texture.Height / 2) <= 0 || _gameData.Sprites[i].Position.Y + (_gameData.Sprites[i].Texture.Height / 2) >= _gameData.Settings.WorldSize)
                //            {
                //                if (_gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                //                    _gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y < 0 ||
                //                    _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                //                    _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y < 0)
                //                {
                //                    _gameData.Sprites[i].Rotation = (((float)Math.PI) - _gameData.Sprites[i].Rotation);
                //                }
                //            }
                //        }
                //    }
                //}

                UpdateMoveSprite(gameTime, i);
            }
        }

        foreach (SpriteBase c in deadSpritesToRemove)
        {
            _gameData.AddDeadSpriteToList(c);
            _gameData.Sprites.Remove(c);
        }
    }
    private void UpdateMoveSprite(GameTime gameTime, int spriteIndex)
    {
        if (_gameData.Sprites[spriteIndex].IsAlive && _gameData.Sprites[spriteIndex].IsMovable())
        {
            //Move the creature
            _gameData.Sprites[spriteIndex].Position += _gameData.Sprites[spriteIndex].Direction * ((_gameData.Sprites[spriteIndex].Speed / 10f) * (_currentTicksPerSecond / TICKS_PER_SECOND)) * TICKS_PER_SECOND * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _gameData.Sprites[spriteIndex].GetGridPositionsForSpriteBase(GRID_CELL_SIZE, _gameData);

            if (_gameData.Sprites[spriteIndex].CurrentGridPositionsForCompare != _gameData.Sprites[spriteIndex].OldGridPositionsForCompare)
            {
                //Remove delta
                List<Point> delta = _gameData.Sprites[spriteIndex].GetGridDelta();
                if (delta.Count > 0)
                {
                    _gameData.RemoveSpriteFromGrid(_gameData.Sprites[spriteIndex], delta);
                }

                //Add delta
                delta = _gameData.Sprites[spriteIndex].GetGridDeltaAdd();
                if (delta.Count > 0)
                {
                    _gameData.AddSpriteDeltaToGrid(_gameData.Sprites[spriteIndex], delta);
                }
            }
        }
    }

    //Draw functions
    private void DrawBorders()
    {
        _spriteBatch.Draw(_borders.Texture, new Rectangle((int)_borders.LeftWall.X - BORDER_WIDTH, (int)_borders.LeftWall.Y, BORDER_WIDTH, _gameData.Settings.WorldSize + BORDER_WIDTH), Color.SaddleBrown);
        _spriteBatch.Draw(_borders.Texture, new Rectangle((int)_borders.RightWall.X, (int)_borders.RightWall.Y - BORDER_WIDTH, BORDER_WIDTH, _gameData.Settings.WorldSize + BORDER_WIDTH), Color.SaddleBrown);
        _spriteBatch.Draw(_borders.Texture, new Rectangle((int)_borders.TopWall.X - BORDER_WIDTH, (int)_borders.TopWall.Y - BORDER_WIDTH, _gameData.Settings.WorldSize + BORDER_WIDTH, BORDER_WIDTH), Color.SaddleBrown);
        _spriteBatch.Draw(_borders.Texture, new Rectangle((int)_borders.BottomWall.X - BORDER_WIDTH, (int)_borders.BottomWall.Y, _gameData.Settings.WorldSize + (BORDER_WIDTH * 2), BORDER_WIDTH), Color.SaddleBrown);
    }
}