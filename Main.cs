using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Design;
using System;
using ProjectMove.Content.Player;
using ProjectMove.Content.Npcs;
using ProjectMove.Content.Levels;
using ProjectMove.Content.Tiles;
using System.Collections.Generic;
using ProjectMove.Content.Gui;
using ProjectMove.Content.Tiles.TileTypes;
using ProjectMove.Content.Tiles.TileTypes.Walls;
using static ProjectMove.GameID;
using ProjectMove.Content.Tiles.TileTypes.Objects;
using ProjectMove.Content.Tiles.TileTypes.Floors;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using ProjectMove.Content;

namespace ProjectMove
{
    public class GameMain : Game //Main Class
    {
        public readonly GraphicsDeviceManager graphics;
        public RenderTarget2D target;
        public static SpriteBatch spriteBatch;
        public static SpriteFont font_Arial;
        public static SpriteFont font_Arial_Bold;

        public static int screenWidth;
        public static int screenHeight;
        public static Point ScreenSize
        {
            get { return new Point(screenWidth, screenHeight); }
        }
        public static int mainUpdateCount;
        public static Random random;

        public static World mainWorld;

        public static float zoom = 1f;
        public Effect zoomEffect;

        public static int selectedFloorTile;
        public static int selectedWallTile;
        public static int selectedObjectTile;

        public static byte buildModeLayer = (byte)TileHandler.TileLayer.Wall;
        public static bool buildMode = false;

        public static bool debug = false;

        public static bool lockCamera = false;

        public static Point cameraPosition;
        public static Point mousePos;

        //textures
        public static Texture2D mouseTexture;
        public static Texture2D debugTexture;
        public static Texture2D playerTexture;

        public static Texture2D BuildUi;
        public static Texture2D BuildUiPreview;
        public static Texture2D BuildUiFloor;
        public static Texture2D BuildUiWall;
        public static Texture2D BuildUiObject;

        public const float spriteScaling = 2f;//since all sprites are scaled to 2x
        public static float uiScaling = 2f;

        public static Game Instance { get; set; }

        public GameMain()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Instance = this;//this is how
        }

        #region loading
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            screenWidth = graphics.PreferredBackBufferWidth;
            screenHeight = graphics.PreferredBackBufferHeight;

            mainTarget = DefaultTarget;

            tileTarget = DefaultTarget;
            entityTarget = DefaultTarget;
            postTileTarget = DefaultTarget;
            screenTarget = DefaultTarget;

            drawingTarget = DefaultTarget;

            random = new Random();


            cameraPosition = Point.Zero;

            mousePos = new Point();

            TextureHandler.Initialize();

            GameID.Initialize();

            NpcHandler.Initialize();
            LevelHandler.Initialize();
            TileHandler.Initialize();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            #region textures, fonts, and shaders
            font_Arial = Content.Load<SpriteFont>("Arial");
            font_Arial_Bold = Content.Load<SpriteFont>("Arial_Bold");



            zoomEffect = Content.Load<Effect>("Effects/Zoom");

            playerTexture = Content.Load<Texture2D>("Player/Player");
            debugTexture = Content.Load<Texture2D>("Debug");

            mouseTexture = Content.Load<Texture2D>("Gui/MousePointer");

            BuildUi = Content.Load<Texture2D>("Gui/build_overlay");
            BuildUiPreview = Content.Load<Texture2D>("Gui/PreviewBorder");
            BuildUiFloor = Content.Load<Texture2D>("Gui/build_overlay_floor");
            BuildUiWall = Content.Load<Texture2D>("Gui/build_overlay_wall");
            BuildUiObject = Content.Load<Texture2D>("Gui/build_overlay_object");

            selectedFloorTile = GetFloorID<AirFloor>();
            selectedWallTile = GetWallID<AirWall>();
            selectedObjectTile = GetObjectID<AirObject>();

            TileHandler.LoadTileTextures();
            NpcHandler.LoadNpcTextures();
            #endregion

            mainWorld = new World();
            mainWorld.Initialize();

            base.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        #endregion

        private bool ButtonJustPressed(Keys keyType)
        {
            if (oldKeyState.IsKeyUp(keyType) && keyState.IsKeyDown(keyType))//is tracking bool is true, and key down: invert bools and return true
            {
                return true;
            }
            return false;
        }

        private void ButtonToggle(ref bool stateBool, Keys keyType)
        {
            if (ButtonJustPressed(keyType))
            {
                stateBool = !stateBool;
            }
        }

        private ref int GetSelectedTile(int layer)
        {
            switch (layer)
            {
                case (int)TileHandler.TileLayer.Floor:
                    return ref selectedFloorTile;
                case (int)TileHandler.TileLayer.Wall:
                    return ref selectedWallTile;
                default:
                    return ref selectedObjectTile;
            }
        }



        public KeyboardState keyState;
        public KeyboardState oldKeyState;

        public MouseState mouseState;
        public MouseState oldMouseState;

        public int ScrollValue
        {
            get => mouseState.ScrollWheelValue;
        }
        public int oldScrollValue;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //closing the game with esc (debug)
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyState.IsKeyDown(Keys.Escape)) { Exit(); }

            mainUpdateCount++;//updating the main counter

            keyState = Keyboard.GetState();//gets keyboard state
            mouseState = Mouse.GetState();

            mousePos = mouseState.Position;

            #region zooming
            if (keyState.IsKeyDown(Keys.OemMinus))
            {
                zoom -= 0.010f;
                if (zoom < 1)
                    zoom = 1;
            }
            else if (keyState.IsKeyDown(Keys.OemPlus))
            {
                zoom += 0.010f;
            }
            #endregion

            #region mode changing buttons (build, debug, etc)
            ButtonToggle(ref debug, Keys.G);

            ButtonToggle(ref lockCamera, Keys.C);

            ButtonToggle(ref buildMode, Keys.B);
            #endregion

            #region build mode functions
            if (buildMode)
            {
                if (ButtonJustPressed(Keys.Up))
                {
                    if (buildModeLayer >= 2)
                        buildModeLayer = 0;
                    else buildModeLayer++;
                }
                if (ButtonJustPressed(Keys.Down))
                {
                    if (buildModeLayer <= 0)
                        buildModeLayer = 2;
                    else buildModeLayer--;
                }

                if (ScrollValue > oldScrollValue || ButtonJustPressed(Keys.Left))
                {
                    ref int selectedTile = ref GetSelectedTile(buildModeLayer);
                    if (selectedTile <= 0)
                        selectedTile = (ushort)TileHandler.TypeCount(buildModeLayer);
                    else
                        selectedTile--;
                }
                else if (ScrollValue < oldScrollValue || ButtonJustPressed(Keys.Right))
                {
                    ref int selectedTile = ref GetSelectedTile(buildModeLayer);
                    if (selectedTile >= TileHandler.TypeCount(buildModeLayer))
                        selectedTile = 0;
                    else
                        selectedTile++;
                }

                Point mouseTileCoord = mousePos.ScreenToTileCoords();
                if (mainWorld.IsTileInWorld(mouseTileCoord))
                {
                    if (mouseState.LeftButton == ButtonState.Pressed)
                    {
                        mainWorld.PlaceTile(GetSelectedTile(buildModeLayer), mouseTileCoord, buildModeLayer);
                    }
                    else if (mouseState.RightButton == ButtonState.Pressed)
                    {
                        mainWorld.PlaceTile(TileHandler.GetAirTile(buildModeLayer), mouseTileCoord, buildModeLayer);
                    }
                }
            }
            #endregion

            mainWorld.Update();//all player and npc updates in the world


            oldKeyState = keyState;
            oldMouseState = mouseState;
            oldScrollValue = ScrollValue;

            base.Update(gameTime);
        }

        public RenderTarget2D DefaultTarget {
            get => new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24); }

        public RenderTarget2D mainTarget;
        public RenderTarget2D tileTarget;
        public RenderTarget2D entityTarget;
        public RenderTarget2D postTileTarget;
        public RenderTarget2D screenTarget;

        public RenderTarget2D drawingTarget;

        public Color backgroundColor = Color.CornflowerBlue; ////new Color(33, 24, 27);

        protected override void Draw(GameTime gameTime)
        {
            //TODO: move tiles to RT and only redraw when needed

            GraphicsDevice.SetRenderTarget(tileTarget);//everything will now draw to the render target
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            mainWorld.DrawTiles(spriteBatch);
            spriteBatch.End();


            GraphicsDevice.SetRenderTarget(entityTarget);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            mainWorld.DrawEntities(spriteBatch);
            spriteBatch.End();


            GraphicsDevice.SetRenderTarget(postTileTarget);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            mainWorld.PostDrawTiles(spriteBatch);
            spriteBatch.End();


            GraphicsDevice.SetRenderTarget(mainTarget);
            GraphicsDevice.Clear(backgroundColor);//background color
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            spriteBatch.Draw(tileTarget, Vector2.Zero, Color.White);
            spriteBatch.Draw(entityTarget, Vector2.Zero, Color.White);
            spriteBatch.Draw(postTileTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            //UI
            GraphicsDevice.SetRenderTarget(screenTarget);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            mainWorld.ExtraDraw(spriteBatch);

            DrawBuildUi(spriteBatch);

            DrawMouse(spriteBatch);

            DrawDebug(spriteBatch, gameTime);

            spriteBatch.End();


            GraphicsDevice.SetRenderTarget(null);
            zoomEffect.Parameters["Zoom"].SetValue(new Vector2(zoom));
            zoomEffect.Parameters["Offset"].SetValue(Vector2.Zero);//if this is used, ScreenToWorldCoords might be changed to account for this
            
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, effect: zoomEffect);
            spriteBatch.Draw(mainTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            //UI
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            spriteBatch.Draw(screenTarget, Vector2.Zero, Color.White);
            spriteBatch.End();
        }

        private void DrawBuildUi(SpriteBatch spriteBatch)
        {
            if (buildMode)
            {
                Texture2D modeTexture = buildModeLayer == (byte)TileHandler.TileLayer.Floor ? BuildUiFloor : 
                                        buildModeLayer == (byte)TileHandler.TileLayer.Wall ? BuildUiWall : 
                                                                                            BuildUiObject;
                spriteBatch.Draw(modeTexture, new Vector2(0, screenHeight), null, Color.White, default, new Vector2(0, modeTexture.Height), uiScaling, default, default);
                spriteBatch.Draw(BuildUi, new Vector2(0, screenHeight), null, Color.White, default, new Vector2(0, BuildUi.Height), uiScaling, default, default);
            }
        }

        private void DrawMouse(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(mouseTexture, mousePos.ToVector2(), null, Color.White, default, default, uiScaling, default, default);

            if (buildMode)
            {
                Vector2 pos = mousePos.ToVector2() + new Vector2(mouseTexture.Width, BuildUiPreview.Height  * 2.1f);
                spriteBatch.Draw(BuildUiPreview, pos, null, Color.White, default, BuildUiPreview.Size().Half(), uiScaling, default, default);
                Texture2D tileTexture = buildModeLayer == (byte)TileHandler.TileLayer.Floor ? TextureHandler.GetTexture(TileHandler.FloorBases[selectedFloorTile].texture) : 
                                        buildModeLayer == (byte)TileHandler.TileLayer.Wall ? TextureHandler.GetTexture(TileHandler.WallBases[selectedWallTile].texture) :
                                                                                            TextureHandler.GetTexture(TileHandler.ObjectBases[selectedObjectTile].texture);
                spriteBatch.Draw(tileTexture, pos, null, Color.White, default, tileTexture.Size().Half(), uiScaling, default, default);

                string tileName = buildModeLayer == (byte)TileHandler.TileLayer.Floor ? TileHandler.FloorBases[selectedFloorTile].GetType().Name :
                                        buildModeLayer == (byte)TileHandler.TileLayer.Wall ? TileHandler.WallBases[selectedWallTile].GetType().Name :
                                                                                            TileHandler.ObjectBases[selectedObjectTile].GetType().Name;
                Vector2 tileNameOffset = font_Arial.MeasureString(tileName);
                spriteBatch.DrawString(font_Arial, tileName, pos + new Vector2(0, tileNameOffset.Y * 2f), Color.White, default, tileNameOffset.Half(), 1.2f, default, default);//position + new Vector2(20, 0) //new Vector2(GameMain.screenWidth / 2, GameMain.screenHeight / 2)
            }

            #region commented debug draws for later
            //Point tileCoord = mousePos.ScreenToTileCoords();

            //npc names need to be drawn on a different layer so they are effected by the zoom
            //foreach (Npc npc in mainWorld.npcs)
            //{
            //    if (npc.Rect.Contains(mousePos.ScreenToWorldCoords()))
            //    {
            //        Vector2 npcNameOffset = font_Arial.MeasureString(npc.displayName);
            //        spriteBatch.DrawString(font_Arial, npc.displayName, npc.Center.WorldToScreenCoords(), Color.White, default, new Vector2(npcNameOffset.X / 2, -npcNameOffset.Y / 2), 1f, default, default);//position + new Vector2(20, 0) //new Vector2(GameMain.screenWidth / 2, GameMain.screenHeight / 2)
            //    }
            //}

            //if (mainWorld.IsTileInWorld(tileCoord))
            //{
            //    //string tileName = TileHandler.WallBases[mainWorld.wallLayer[tileCoord.X, tileCoord.Y].type].GetType().Name;
            //    string tileName = TileHandler.ObjectBases[mainWorld.objectLayer[tileCoord.X, tileCoord.Y].type].GetType().Name;
            //    //string tileName = TileHandler.FloorBases[mainWorld.floorLayer[tileCoord.X, tileCoord.Y].type].IsSolid().ToString();

            //    Vector2 tileNameOffset = font_Arial.MeasureString(tileName);
            //    spriteBatch.DrawString(font_Arial, tileName, mousePos.ToVector2(), Color.White, default, new Vector2(tileNameOffset.X / 2.5f, tileNameOffset.Y), 1f, default, default);
            //}

            //spriteBatch.Draw(debugTexture, mousePos.ToVector2(), Color.BlueViolet);//debug
            //spriteBatch.Draw(debugTexture, mousePos.ScreenToWorldCoords().WorldToScreenCoords(), Color.IndianRed);//debug
            #endregion
        }

        private readonly List<float> fps = new List<float>();

        private void DrawDebug(SpriteBatch spriteBatch, GameTime gameTime)
        {
            fps.Add(1f / ((float)gameTime.ElapsedGameTime.Milliseconds / 1000));

            float counter = 0;
            foreach(float num in fps)
            {
                counter += num;
            }

            spriteBatch.DrawString(font_Arial, "FPS: " + Math.Round(counter / fps.Count, 2).ToString(), ScreenSize.ToVector2() / 40, Color.LightGoldenrodYellow, default, default, 1f, default, default);

            if (fps.Count >= 15)//how many frames to average
            {
                fps.RemoveAt(0);
            }

            if (debug)
            {
                string str = "Zoom: " + Math.Round(zoom, 2).ToString();
                Vector2 textSize = font_Arial.MeasureString(str);
                spriteBatch.DrawString(font_Arial, str, new Vector2(ScreenSize.X - (textSize.X + 20), ScreenSize.Y / 40), Color.LightGoldenrodYellow, default, default, 1f, default, default); ;//position + new Vector2(20, 0) //new Vector2(GameMain.screenWidth / 2, GameMain.screenHeight / 2)
            }
        }
    }
}
