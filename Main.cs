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
using ProjectMove.Content.Tiles.TileTypes.Floor;
using ProjectMove.Content.Tiles.TileTypes.Floors;

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

        public static Texture2D mouseTexture;
        public static Point mousePos;


        //camera
        public static Point cameraPosition;
        public static bool lockCamera = false;
        private bool hasReleasedCameraButton = true;

        public static float zoom = 1f;
        public Effect zoomEffect;

        public RenderTarget2D mainTarget;

        public RenderTarget2D tileTarget;

        public RenderTarget2D entityTarget;

        public RenderTarget2D postTileTarget;

        public RenderTarget2D screenTarget;

        //debug variables
        public static bool debug = false;
        private bool hasReleasedDebugButton = true;

        //textures
        public static Texture2D debugTexture;
        public static Texture2D playerTexture;

        public const float spriteScaling = 2f;

        public static Game Instance { get; set; }

        public GameMain()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Instance = this;
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

            mainTarget = new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

            tileTarget = new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            entityTarget = new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            postTileTarget = new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            screenTarget = new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

            cameraPosition = Point.Zero;

            random = new Random();

            mousePos = new Point();

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

            TileHandler.LoadTileTextures();
            NpcHandler.LoadNpcTextures();
            #endregion

            mainWorld = new World();
            mainWorld.Initialize();

            base.LoadContent();
        }

        [Obsolete("Use extension instead")]
        public static void LoadObjectTextures(ref Texture2D[] texArray, ref List<string> nameArray, string directory)
        {
            texArray = new Texture2D[nameArray.Count];
            for (int i = 0; i < nameArray.Count; i++)
            {
                try
                {
                    texArray[i] = Instance.Content.Load<Texture2D>(directory + nameArray[i]);
                }
                catch (ContentLoadException)
                {
                    System.Diagnostics.Debug.WriteLine("Missing Texture: " + directory + nameArray[i]   );
                    texArray[i] = Instance.Content.Load<Texture2D>("Debug1");//fallback to this texture
                }
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
        #endregion


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            mainUpdateCount++;//updating the main counter

            KeyboardState keyState = Keyboard.GetState();//gets keyboard state

            MouseState mouseState = Mouse.GetState();

            mousePos = mouseState.Position;

            //closing the game with esc (debug)
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyState.IsKeyDown(Keys.Escape)) { Exit();}

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

            //toggling debug mode
            ButtonToggle(ref debug, ref hasReleasedDebugButton, ref keyState, Keys.G);

            //toggle locking the camera
            ButtonToggle(ref lockCamera, ref hasReleasedCameraButton, ref keyState, Keys.C);

            if (debug)
            {
                Point tileCoord = mousePos.ScreenToTileCoords();
                if (mainWorld.IsTileInWorld(tileCoord))
                {
                    if (mouseState.LeftButton == ButtonState.Pressed)
                    {
                        mainWorld.PlaceTile(GetObjectID<Desk>(), tileCoord, (int)World.TileLayer.Object);
                    }
                    else if (mouseState.RightButton == ButtonState.Pressed)
                    {
                        mainWorld.PlaceTile(GetWallID<AirWall>(), tileCoord, (int)World.TileLayer.Wall);
                    }
                }
            }

            mainWorld.Update();//all player and npc updates in the world


            base.Update(gameTime);
        }

        private void ButtonToggle(ref bool stateBool, ref bool buttonTrack, ref KeyboardState keystate, Keys keyType)
        {
            if (buttonTrack && keystate.IsKeyDown(keyType))
            {
                stateBool = !stateBool;
                buttonTrack = false;
            }
            else if (!buttonTrack && keystate.IsKeyUp(keyType))
            {
                buttonTrack = true;
            }
        }

        #region debug draws
        //spriteBatch.Draw(debugTexture, mousePos.ToVector2(), Color.BlueViolet);//debug
        //spriteBatch.Draw(debugTexture, mousePos.ScreenToWorldCoords().WorldToScreenCoords(), Color.IndianRed);//debug
        #endregion

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(tileTarget);//everything will now draw to the render target
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            mainWorld.DrawTiles(spriteBatch);
            spriteBatch.End();


            GraphicsDevice.SetRenderTarget(entityTarget);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            mainWorld.DrawEntities(spriteBatch);
            spriteBatch.End();


            GraphicsDevice.SetRenderTarget(postTileTarget);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            mainWorld.PostDrawTiles(spriteBatch);
            spriteBatch.End();


            GraphicsDevice.SetRenderTarget(mainTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);//background color
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            spriteBatch.Draw(tileTarget, Vector2.Zero, Color.White);
            spriteBatch.Draw(entityTarget, Vector2.Zero, Color.White);
            spriteBatch.Draw(postTileTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(screenTarget);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            mainWorld.ExtraDraw(spriteBatch);
            #region mouse
            spriteBatch.Draw(mouseTexture, mousePos.ToVector2(), null, Color.White, default, default, 2f, default, default);

            foreach (Npc npc in mainWorld.npcs)
            {
                if (npc.Rect.Contains(mousePos.ScreenToWorldCoords()))
                {
                    Vector2 npcNameOffset = font_Arial.MeasureString(npc.displayName);
                    spriteBatch.DrawString(font_Arial, npc.displayName, mousePos.ToVector2(), Color.White, default, new Vector2(npcNameOffset.X / 2.5f, -npcNameOffset.Y), 1f, default, default);//position + new Vector2(20, 0) //new Vector2(GameMain.screenWidth / 2, GameMain.screenHeight / 2)
                }
            }

            Point tileCoord = mousePos.ScreenToTileCoords();

            if (mainWorld.IsTileInWorld(tileCoord))
            {
                //string tileName = TileHandler.WallBases[mainWorld.wallLayer[tileCoord.X, tileCoord.Y].type].GetType().Name;
                string tileName = TileHandler.ObjectBases[mainWorld.objectLayer[tileCoord.X, tileCoord.Y].type].GetType().Name;
                //string tileName = TileHandler.FloorBases[mainWorld.floorLayer[tileCoord.X, tileCoord.Y].type].IsSolid().ToString();

                Vector2 tileNameOffset = font_Arial.MeasureString(tileName);
                spriteBatch.DrawString(font_Arial, tileName, mousePos.ToVector2(), Color.White, default, new Vector2(tileNameOffset.X / 2.5f, tileNameOffset.Y), 1f, default, default);
            }
            //spriteBatch.DrawString(font_Arial, mousePos.ScreenToWorldCoords().ToString(), mousePos.ToVector2(), Color.White, default, default, 1f, default, default); ;

            #endregion
            if (debug)//fps
            {
                spriteBatch.DrawString(font_Arial, "FPS: " + Math.Round(1f / gameTime.ElapsedGameTime.TotalSeconds, 2).ToString(), ScreenSize.ToVector2() / 40, Color.LightGoldenrodYellow, default, default, 1f, default, default); ;//position + new Vector2(20, 0) //new Vector2(GameMain.screenWidth / 2, GameMain.screenHeight / 2)
                                                                                                                                                                                                                                      //un-rounded version //spriteBatch.DrawString(font_Arial, "FPS: " + (1f / gameTime.ElapsedGameTime.TotalSeconds).ToString(), ScreenSize / 40, Color.LightGoldenrodYellow, default, default, 1f, default, default); ;//position + new Vector2(20, 0) //new Vector2(GameMain.screenWidth / 2, GameMain.screenHeight / 2)

                string str = "Zoom: " + Math.Round(zoom, 2).ToString();
                //string str = "Zoom: " + GetFloorID<BrickFloor>();
                Vector2 textSize = font_Arial.MeasureString(str);
                spriteBatch.DrawString(font_Arial, str, new Vector2(ScreenSize.X - (textSize.X + 20), ScreenSize.Y / 40), Color.LightGoldenrodYellow, default, default, 1f, default, default); ;//position + new Vector2(20, 0) //new Vector2(GameMain.screenWidth / 2, GameMain.screenHeight / 2)
            }
            spriteBatch.End();


            GraphicsDevice.SetRenderTarget(null);
            zoomEffect.Parameters["Zoom"].SetValue(new Vector2(zoom));
            zoomEffect.Parameters["Offset"].SetValue(Vector2.Zero);//if this is used, ScreenToWorldCoords might be changed to account for this
            
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, effect: zoomEffect);
            spriteBatch.Draw(mainTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            //UI
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            spriteBatch.Draw(screenTarget, Vector2.Zero, Color.White);
            spriteBatch.End();
        }
    }
}
