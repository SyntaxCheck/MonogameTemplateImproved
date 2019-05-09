using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

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
    private int _frames;
    private double _elapsedSeconds;
    private int _fps;
    private double _elapsedSecondsSinceTick;
    private double _elapsedTimeSinceFoodGeneration;
    private float _tickSeconds;
    private float _elapsedTicksSinceSecondProcessing;
    //Constants
    private const int GRID_CELL_SIZE = 50; //Seems to be the sweet spot for a 5,000 x 5,000 map based on the texture sizes we have so far
    private const int BORDER_WIDTH = 10;
    private const float TICKS_PER_SECOND = 30; //How many ticks per second we should have
    private const bool ENABLE_DEBUG = false;
    //Colors
    private Color MAP_COLOR = Color.AliceBlue;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferHeight = 900;
        _graphics.PreferredBackBufferWidth = 1600;
        _graphics.SynchronizeWithVerticalRetrace = false; //Unlock fps
        this.IsFixedTimeStep = false; //Unlock fps

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
        Texture2D whitePixel = new Texture2D(_graphics.GraphicsDevice, 1, 1);
        Color[] color = new Color[1];
        color[0] = Color.White;
        whitePixel.SetData(color);

        //_gameData.Textures.Add(new TextureContainer() { Texture = _whitePixel, Name = TextureName.WhitePixel }); //Adding texture when using the Container as a list
        _gameData.Textures.WhitePixel = whitePixel;
        _gameData.Textures.ParticleCollisionWestTopSpriteSheet = Content.Load<Texture2D>(@"Animations\ImpactWestTop");
        _gameData.Textures.ParticleCollisionWestBottomSpriteSheet = Content.Load<Texture2D>(@"Animations\ImpactWestBottom");
        _gameData.Textures.ParticleCollisionEastTopSpriteSheet = Content.Load<Texture2D>(@"Animations\ImpactEastTop");
        _gameData.Textures.ParticleCollisionEastBottomSpriteSheet = Content.Load<Texture2D>(@"Animations\ImpactEastBottom");
        _gameData.Textures.ParticleCollisionNorthLeftSpriteSheet = Content.Load<Texture2D>(@"Animations\ImpactNorthLeft");
        _gameData.Textures.ParticleCollisionNorthRightSpriteSheet = Content.Load<Texture2D>(@"Animations\ImpactNorthRight");
        _gameData.Textures.ParticleCollisionSouthLeftSpriteSheet = Content.Load<Texture2D>(@"Animations\ImpactSouthLeft");
        _gameData.Textures.ParticleCollisionSouthRightSpriteSheet = Content.Load<Texture2D>(@"Animations\ImpactSouthRight");
        _gameData.Textures.ClickExplosionSpriteSheet = Content.Load<Texture2D>(@"Animations\ClickExplosion1");

        _rand = new Random();
        _gameData.CollisionAnimFactory = new CollisionParticleAnimationFactory(_gameData.Textures.ParticleCollisionEastBottomSpriteSheet, _gameData.Textures.ParticleCollisionEastTopSpriteSheet, _gameData.Textures.ParticleCollisionWestBottomSpriteSheet, _gameData.Textures.ParticleCollisionWestTopSpriteSheet, _gameData.Textures.ParticleCollisionNorthLeftSpriteSheet, _gameData.Textures.ParticleCollisionNorthRightSpriteSheet, _gameData.Textures.ParticleCollisionSouthLeftSpriteSheet, _gameData.Textures.ParticleCollisionSouthRightSpriteSheet, 4, 40);
        _gameData.OnClickAnimFactory = new OnClickAnimationFactory(_gameData.Textures.ClickExplosionSpriteSheet, 4, 30);

        //Generate the Map
        _borders = new Borders();
        _borders.Texture = whitePixel;
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

        //SpawnScenerioTestObjs();

        for (int i = 0; i < 500; i++)
        {
            SpawnTestObject();
        }
        for (int i = 0; i < 200; i++)
        {
            SpawnSampleBuilding();
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
            _player.HandleInput(_inputState, ref _gameData);
            Global.Camera.HandleInput(_inputState, PlayerIndex.One, gameTime, ref _gameData);

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

        UpdateHandleObjectsToBeDrawn(gameTime);

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
        DrawWorldObjects();

        //DRAW HUD INFOMATION
        DrawHUD();

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
        _tickSeconds = 1 / TICKS_PER_SECOND;
    }

    //Update functions
    private void UpdateTick(GameTime gameTime)
    {
        _elapsedTicksSinceSecondProcessing++;

        UpdateTickSprites(gameTime);
    }
    private void UpdateOffTick(GameTime gameTime)
    {
        UpdateOffTickHandleCollisionsAndMovement(gameTime); //Collisions And Movement
        UpdateAnimations(gameTime); //Run animation

        //Every second interval processing only when it is not a TICK. When things only need to be updated once every X seconds
        if (_elapsedTicksSinceSecondProcessing >= TICKS_PER_SECOND * 5)
        {
            UpdateOffTickInterval(gameTime);
        }
    }
    private void UpdateOffTickInterval(GameTime gameTime)
    {
        _elapsedTicksSinceSecondProcessing = 0;
        UpdateOffTickIntervalCleanupAnimations(gameTime);
    }

    private void UpdateTickSprites(GameTime gameTime)
    {
        for (int i = _gameData.Sprites.Count - 1; i >= 0; i--)
        {
            UpdateMoveSprite(gameTime, i);
        }
    }
    private void UpdateOffTickHandleCollisionsAndMovement(GameTime gameTime)
    {
        List<SpriteBase> deadSpritesToRemove = new List<SpriteBase>();

        //CollisionDetection
        //Border Collision Detection
        for (int i = 0; i < _gameData.Sprites.Count; i++)
        {
            if (_gameData.Sprites[i].IsAlive && _gameData.Sprites[i].IsMovable)
            {
                //Change rotation on wall collision
                if (_gameData.Sprites[i].Bounds.Left <= 0 || _gameData.Sprites[i].Bounds.Right >= _gameData.Settings.WorldSize)
                {
                    if (_gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                        _gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y < 0 ||
                        _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                        _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y < 0)
                    {
                        _gameData.Sprites[i].Rotation = (((float)Math.PI * 2) - _gameData.Sprites[i].Rotation);
                    }
                }
                if (_gameData.Sprites[i].Bounds.Top <= 0 || _gameData.Sprites[i].Bounds.Bottom >= _gameData.Settings.WorldSize)
                {
                    if (_gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                        _gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y < 0 ||
                        _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                        _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y < 0)
                    {
                        _gameData.Sprites[i].Rotation = (((float)Math.PI) - _gameData.Sprites[i].Rotation);
                    }
                }

                //Stationary object collision
                foreach (Point p in _gameData.Sprites[i].GridPositions)
                {
                    for (int k = (_gameData.MapGridData[p.X, p.Y].Sprites.Count() - 1); k >= 0; k--)
                    {
                        if (!_gameData.MapGridData[p.X, p.Y].Sprites[k].IsMovable && _gameData.Sprites[i].Bounds.Intersects(_gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds))
                        {
                            Vector2 offset = CollisionDetection.GetIntersectionDepth(_gameData.Sprites[i].Bounds, _gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds);

                            //Add a animation for the collision before we adjust the rotation only if the object is on screen
                            if(_gameData.MapGridData[p.X, p.Y].Sprites[k].DrawObject)
                                _gameData.Animations.AddRange(_gameData.CollisionAnimFactory.Build(_gameData.Sprites[i].Bounds, _gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds, offset, _gameData.Sprites[i].Direction, _gameData.Sprites[i].Scale, Color.Gray));

                            if (Math.Abs(offset.X) < Math.Abs(offset.Y))
                            {
                                if (_gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                                    _gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y < 0 ||
                                    _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                                    _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y < 0)
                                {
                                    _gameData.Sprites[i].Rotation = (((float)Math.PI * 2) - _gameData.Sprites[i].Rotation);
                                    _gameData.Sprites[i].Position = new Vector2(_gameData.Sprites[i].Position.X + offset.X, _gameData.Sprites[i].Position.Y);
                                }
                            }
                            else
                            {
                                if (_gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                                    _gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y < 0 ||
                                    _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                                    _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y < 0)
                                {
                                    _gameData.Sprites[i].Rotation = (((float)Math.PI) - _gameData.Sprites[i].Rotation);
                                    _gameData.Sprites[i].Position = new Vector2(_gameData.Sprites[i].Position.X, _gameData.Sprites[i].Position.Y + offset.Y);
                                }
                            }
                        }
                    }
                }

                //Moving Object Collision
                foreach (Point p in _gameData.Sprites[i].GridPositions)
                {
                    for (int k = (_gameData.MapGridData[p.X, p.Y].Sprites.Count() - 1); k >= 0; k--)
                    {
                        if (_gameData.Sprites[i] != _gameData.MapGridData[p.X, p.Y].Sprites[k] && _gameData.Sprites[i].Bounds.Intersects(_gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds))
                        {
                            //Vector2 offset = CollisionDetection.GetIntersectionDepth(_gameData.Sprites[i].Bounds, _gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds);

                            //_gameData.Sprites[i].Position = new Vector2(_gameData.Sprites[i].Position.X + offset.X, _gameData.Sprites[i].Position.Y + offset.Y);

                            //if (Math.Abs(offset.X) < Math.Abs(offset.Y))
                            //{
                            //    _gameData.Sprites[i].Rotation = (((float)Math.PI * 2) - _gameData.Sprites[i].Rotation);
                            //}
                            //else
                            //{
                            //    _gameData.Sprites[i].Rotation = (((float)Math.PI) - _gameData.Sprites[i].Rotation);
                            //}

                            ////Change rotation on object collision just for a sample
                            //if ((_gameData.Sprites[i].Bounds.X > _gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds.X && _gameData.Sprites[i].Bounds.Left <= _gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds.Right))
                            //{
                            //    //_gameData.Sprites[i].Position = new Vector2(_gameData.Sprites[i].Position.X + (_gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds.Right - _gameData.Sprites[i].Bounds.Left), _gameData.Sprites[i].Position.Y);
                            //    _gameData.Sprites[i].Rotation = (((float)Math.PI * 2) - _gameData.Sprites[i].Rotation);
                            //}
                            //else if ((_gameData.Sprites[i].Bounds.X < _gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds.X && _gameData.Sprites[i].Bounds.Right >= _gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds.Left))
                            //{
                            //    //_gameData.Sprites[i].Position = new Vector2(_gameData.Sprites[i].Position.X - (_gameData.Sprites[i].Bounds.Right - _gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds.Left), _gameData.Sprites[i].Position.Y);
                            //    _gameData.Sprites[i].Rotation = (((float)Math.PI * 2) - _gameData.Sprites[i].Rotation);
                            //}

                                //if ((_gameData.Sprites[i].Bounds.Y > _gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds.Y && _gameData.Sprites[i].Bounds.Top <= _gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds.Bottom) ||
                                //    (_gameData.Sprites[i].Bounds.Y < _gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds.Y && _gameData.Sprites[i].Bounds.Bottom >= _gameData.MapGridData[p.X, p.Y].Sprites[k].Bounds.Top))
                                //{
                                //    _gameData.Sprites[i].Rotation = (((float)Math.PI) - _gameData.Sprites[i].Rotation);
                                //}

                                //if (_gameData.Sprites[i].Position.X - (_gameData.Sprites[i].Texture.Width / 2) <= 0 || _gameData.Sprites[i].Position.X + (_gameData.Sprites[i].Texture.Width / 2) >= _gameData.Settings.WorldSize)
                                //{
                                //    if (_gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                                //        _gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y < 0 ||
                                //        _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                                //        _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y < 0)
                                //    {
                                //        _gameData.Sprites[i].Rotation = (((float)Math.PI * 2) - _gameData.Sprites[i].Rotation);
                                //    }
                                //}
                                //if (_gameData.Sprites[i].Position.Y - (_gameData.Sprites[i].Texture.Height / 2) <= 0 || _gameData.Sprites[i].Position.Y + (_gameData.Sprites[i].Texture.Height / 2) >= _gameData.Settings.WorldSize)
                                //{
                                //    if (_gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                                //        _gameData.Sprites[i].Direction.X >= 0 && _gameData.Sprites[i].Direction.Y < 0 ||
                                //        _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y >= 0 ||
                                //        _gameData.Sprites[i].Direction.X < 0 && _gameData.Sprites[i].Direction.Y < 0)
                                //    {
                                //        _gameData.Sprites[i].Rotation = (((float)Math.PI) - _gameData.Sprites[i].Rotation);
                                //    }
                                //}
                        }
                    }
                }

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
        if (_gameData.Sprites[spriteIndex].IsAlive && _gameData.Sprites[spriteIndex].IsMovable)
        {
            //Move the creature
            _gameData.Sprites[spriteIndex].Update(gameTime); //Movement done in Update
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
    private void UpdateAnimations(GameTime gameTime)
    {
        for (int i = 0; i < _gameData.Animations.Count(); i++)
        {
            _gameData.Animations[i].Update(gameTime);
        }
    }
    private void UpdateHandleObjectsToBeDrawn(GameTime gameTime)
    {
        for (int i = 0; i < _gameData.Sprites.Count(); i++)
        {
            if (_gameData.Sprites[i].IsAlive && Global.Camera.VisibleArea.Contains(_gameData.Sprites[i].Position))
            {
                _gameData.Sprites[i].DrawObject = true;
            }
            else
            {
                _gameData.Sprites[i].DrawObject = false;
            }
        }
    } //Increase FPS by not drawing offscreen objects
    private void UpdateHandleCameraChange(GameTime gameTime) //Unimplemented
    {
        if (Global.Camera.CameraChange)
        {
            Global.Camera.CameraChange = false;

            //Add logic on Camera Change
        }
    }
    private void UpdateOffTickIntervalCleanupAnimations(GameTime gameTime)
    {
        //Cleanup old animations
        for (int i = _gameData.Animations.Count() - 1; i >= 0; i--)
        {
            if (_gameData.Animations[i].IsAnimationComplete)
            {
                _gameData.Animations[i] = null;
                _gameData.Animations.RemoveAt(i);
            }
        }
    }

    //Draw functions
    public void DrawWorldObjects()
    {
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Global.Camera.TranslationMatrix);
        DrawSprites();
        DrawAnimations();
        DrawBorders();
        DrawDebugData();
        _spriteBatch.End();
    }
    private void DrawHUD()
    {
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
        DrawFps();
        _spriteBatch.End();
    }
    private void DrawSprites()
    {
        //Draw the stationary objects first
        for (int i = 0; i < _gameData.Sprites.Count; i++)
        {
            if (_gameData.Sprites[i].DrawObject && !_gameData.Sprites[i].IsMovable)
                _gameData.Sprites[i].Draw(_spriteBatch);

            if (ENABLE_DEBUG)
            {
                if(_gameData.Sprites[i] == _gameData.Focus)
                    _gameData.Sprites[i].DrawDebugDataForSprite(_spriteBatch, false);

                _gameData.Sprites[i].DrawDebugOutlineForSprite(_spriteBatch);
            }
        }
        //Draw the objects that can move next
        for (int i = 0; i < _gameData.Sprites.Count; i++)
        {
            if(_gameData.Sprites[i].DrawObject && _gameData.Sprites[i].IsMovable)
                _gameData.Sprites[i].Draw(_spriteBatch);

            if (ENABLE_DEBUG)
            {
                if (_gameData.Sprites[i] == _gameData.Focus)
                    _gameData.Sprites[i].DrawDebugDataForSprite(_spriteBatch, false);

                _gameData.Sprites[i].DrawDebugOutlineForSprite(_spriteBatch);
            }
        }
    }
    private void DrawAnimations()
    {
        foreach (AnimationBase a in _gameData.Animations)
        {
            if(!a.IsAnimationComplete)
                a.Draw(_spriteBatch);
        }
    }
    private void DrawBorders()
    {
        _spriteBatch.Draw(_borders.Texture, new Rectangle((int)_borders.LeftWall.X - BORDER_WIDTH, (int)_borders.LeftWall.Y, BORDER_WIDTH, _gameData.Settings.WorldSize + BORDER_WIDTH), Color.SaddleBrown);
        _spriteBatch.Draw(_borders.Texture, new Rectangle((int)_borders.RightWall.X, (int)_borders.RightWall.Y - BORDER_WIDTH, BORDER_WIDTH, _gameData.Settings.WorldSize + BORDER_WIDTH), Color.SaddleBrown);
        _spriteBatch.Draw(_borders.Texture, new Rectangle((int)_borders.TopWall.X - BORDER_WIDTH, (int)_borders.TopWall.Y - BORDER_WIDTH, _gameData.Settings.WorldSize + BORDER_WIDTH, BORDER_WIDTH), Color.SaddleBrown);
        _spriteBatch.Draw(_borders.Texture, new Rectangle((int)_borders.BottomWall.X - BORDER_WIDTH, (int)_borders.BottomWall.Y, _gameData.Settings.WorldSize + (BORDER_WIDTH * 2), BORDER_WIDTH), Color.SaddleBrown);
    }
    private void DrawFps()
    {
        _spriteBatch.DrawString(_diagFont, "FPS: " + _fps, new Vector2(10, 10), Color.Black); //FPS Counter in top left corner
    }
    private void DrawDebugData()
    {
        if (ENABLE_DEBUG)
        {
            //Draw Map Grid
            for (int i = 0; i < _gameData.MapGridData.GetLength(0); i++)
            {
                _spriteBatch.Draw(_gameData.Textures.WhitePixel, new Rectangle(i * GRID_CELL_SIZE, 0, 1, _gameData.Settings.WorldSize), Color.Red);
            }
            for (int i = 0; i < _gameData.MapGridData.GetLength(1); i++)
            {
                _spriteBatch.Draw(_gameData.Textures.WhitePixel, new Rectangle(0, i * GRID_CELL_SIZE, _gameData.Settings.WorldSize, 1), Color.Red);
            }
        }
    }

    //Debug Test functions
    private void SpawnTestObject()
    {
        SpriteBase sprite;

        int randNum = _rand.Next(0, 10);
        if (randNum > 7)
        {
            sprite = new Truck();
            sprite.Scale = (float)(_rand.NextDouble() * 3);
            sprite.Color = Color.Blue;
            sprite.ScreenDepth = 1f; //Only used when you specify FrontToBack or BackToFront on the SpriteBatch
        }
        else if (randNum > 2)
        {
            sprite = new Car();
            sprite.Scale = (float)(_rand.NextDouble() * 1.5);
            sprite.Color = Color.Red;
            sprite.ScreenDepth = 0.5f; //Only used when you specify FrontToBack or BackToFront on the SpriteBatch
        }
        else
        {
            sprite = new Car();
            sprite.Scale = (float)(_rand.NextDouble() * 0.9);
            sprite.Color = Color.Green;
            sprite.ScreenDepth = 0.25f; //Only used when you specify FrontToBack or BackToFront on the SpriteBatch
        }

        if (sprite.Scale < 0.5f)
            sprite.Scale = 0.5f;

        sprite.IsAlive = true;
        sprite.WorldSize = _gameData.Settings.WorldSize;
        sprite.Texture = BuildSampleImage(_graphics.GraphicsDevice);
        sprite.Speed = 150f;
        sprite.Rotation = MathHelper.ToRadians(_rand.Next(0, 360));
        sprite.Position = new Vector2(_rand.Next((int)sprite.AdjustedSize.X, _gameData.Settings.WorldSize - (int)sprite.AdjustedSize.X), _rand.Next((int)sprite.AdjustedSize.Y, _gameData.Settings.WorldSize - (int)sprite.AdjustedSize.Y));
        sprite.GetGridPositionsForSpriteBase(GRID_CELL_SIZE, _gameData);

        //Debug Properties
        sprite.WhiteTexture = _gameData.Textures.WhitePixel; //Used to create debug information
        sprite.DebugFont = _diagFont;

        _gameData.Sprites.Add(sprite);
        _gameData.AddSpriteToGrid(sprite);
    }
    private void SpawnSampleBuilding()
    {
        SpriteBase sprite;

        sprite = new Building();
        sprite.Scale = (float)(_rand.NextDouble() * 3);
        sprite.Color = Color.White;
        sprite.ScreenDepth = 1f; //Only used when you specify FrontToBack or BackToFront on the SpriteBatch
        sprite.IsAlive = true;
        sprite.WorldSize = _gameData.Settings.WorldSize;
        sprite.Texture = BuildSampleImageBuilding(_graphics.GraphicsDevice);
        sprite.Speed = 0f;
        sprite.Rotation = 0;
        sprite.Position = new Vector2(_rand.Next((int)sprite.AdjustedSize.X, _gameData.Settings.WorldSize - (int)sprite.AdjustedSize.X), _rand.Next((int)sprite.AdjustedSize.Y, _gameData.Settings.WorldSize - (int)sprite.AdjustedSize.Y));
        sprite.GetGridPositionsForSpriteBase(GRID_CELL_SIZE, _gameData);

        //Debug Properties
        sprite.WhiteTexture = _gameData.Textures.WhitePixel; //Used to create debug information
        sprite.DebugFont = _diagFont;

        _gameData.Sprites.Add(sprite);
        _gameData.AddSpriteToGrid(sprite);
    }
    public void SpawnScenerioTestObjs()
    {
        SpriteBase sprite;

        sprite = new Truck();
        sprite.Scale = 2.5f;
        sprite.Color = Color.Blue;
        sprite.ScreenDepth = 1f; //Only used when you specify FrontToBack or BackToFront on the SpriteBatch
        if (sprite.Scale < 0.5f)
            sprite.Scale = 0.5f;

        sprite.IsAlive = true;
        sprite.WorldSize = _gameData.Settings.WorldSize;
        sprite.Texture = BuildSampleImage(_graphics.GraphicsDevice);
        sprite.Speed = 100f;
        sprite.Rotation = MathHelper.ToRadians(90);
        sprite.Position = new Vector2(550, 450);
        sprite.GetGridPositionsForSpriteBase(GRID_CELL_SIZE, _gameData);

        //Debug Properties
        sprite.WhiteTexture = _gameData.Textures.WhitePixel; //Used to create debug information
        sprite.DebugFont = _diagFont;

        _gameData.Sprites.Add(sprite);
        _gameData.AddSpriteToGrid(sprite);

        sprite = new Truck();
        sprite.Scale = .8f;
        sprite.Color = Color.Blue;
        sprite.ScreenDepth = 1f; //Only used when you specify FrontToBack or BackToFront on the SpriteBatch
        if (sprite.Scale < 0.5f)
            sprite.Scale = 0.5f;

        sprite.IsAlive = true;
        sprite.WorldSize = _gameData.Settings.WorldSize;
        sprite.Texture = BuildSampleImage(_graphics.GraphicsDevice);
        sprite.Speed = 190f;
        sprite.Rotation = MathHelper.ToRadians(90);
        sprite.Position = new Vector2(550, 570);
        sprite.GetGridPositionsForSpriteBase(GRID_CELL_SIZE, _gameData);

        //Debug Properties
        sprite.WhiteTexture = _gameData.Textures.WhitePixel; //Used to create debug information
        sprite.DebugFont = _diagFont;

        _gameData.Sprites.Add(sprite);
        _gameData.AddSpriteToGrid(sprite);

        //*************************
        //Building
        //*************************

        sprite = new Building();
        sprite.Scale = 3.2f;
        sprite.Color = Color.Black;
        sprite.ScreenDepth = 1f; //Only used when you specify FrontToBack or BackToFront on the SpriteBatch
        sprite.IsAlive = true;
        sprite.WorldSize = _gameData.Settings.WorldSize;
        sprite.Texture = BuildSampleImageBuilding(_graphics.GraphicsDevice);
        sprite.Speed = 0f;
        sprite.Rotation = 0f;
        sprite.Position = new Vector2(450,500);
        sprite.GetGridPositionsForSpriteBase(GRID_CELL_SIZE, _gameData);

        //Debug Properties
        sprite.WhiteTexture = _gameData.Textures.WhitePixel; //Used to create debug information
        sprite.DebugFont = _diagFont;

        _gameData.Sprites.Add(sprite);
        _gameData.AddSpriteToGrid(sprite);

        sprite = new Building();
        sprite.Scale = 2.8f;
        sprite.Color = Color.Black;
        sprite.ScreenDepth = 1f; //Only used when you specify FrontToBack or BackToFront on the SpriteBatch
        sprite.IsAlive = true;
        sprite.WorldSize = _gameData.Settings.WorldSize;
        sprite.Texture = BuildSampleImageBuilding(_graphics.GraphicsDevice);
        sprite.Speed = 0f;
        sprite.Rotation = 0f;
        sprite.Position = new Vector2(800, 500);
        sprite.GetGridPositionsForSpriteBase(GRID_CELL_SIZE, _gameData);

        //Debug Properties
        sprite.WhiteTexture = _gameData.Textures.WhitePixel; //Used to create debug information
        sprite.DebugFont = _diagFont;

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
            pixels.Add(Color.White);
        }

        texture.SetData(pixels.ToArray());

        return texture;
    }
    private Texture2D BuildSampleImageBuilding(GraphicsDevice device)
    {
        int IMAGE_WIDTH = 64;
        int IMAGE_HEIGHT = 64;
        Texture2D texture = new Texture2D(device, IMAGE_WIDTH, IMAGE_HEIGHT);

        List<Color> pixels = new List<Color>();
        int border = 5;

        for (int i = 0; i < IMAGE_HEIGHT; i++)
        {
            for (int k = 0; k < IMAGE_WIDTH; k++)
            {
                if ((i < border || i >= (IMAGE_HEIGHT - border)) || (k < border || k >= (IMAGE_WIDTH - border)))
                {
                    pixels.Add(Color.Black);
                }
                else
                {
                    pixels.Add(Color.White);
                }
            }
        }

        texture.SetData(pixels.ToArray());

        return texture;
    }
}