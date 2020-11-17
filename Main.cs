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
using static ProjectMove.GameID;

namespace ProjectMove
{
    public class GameMain : Game //Main Class
    {
        public readonly GraphicsDeviceManager graphics;
        public static SpriteBatch spriteBatch;
        public static SpriteFont font_Arial;
        public static SpriteFont font_Arial_Bold;

        public static int screenWidth;
        public static int screenHeight;
        public static Vector2 ScreenSize
        {
            get { return new Vector2(screenWidth, screenHeight); }
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

        //debug variables
        public static bool debug = false;
        private bool hasReleasedDebugButton = true;

        //textures
        public static Texture2D debugTexture;
        public static Texture2D playerTexture;

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
            spriteBatch = new SpriteBatch(GraphicsDevice); // Create a new SpriteBatch, which can be used to draw textures.

            #region textures and fonts
            font_Arial = Content.Load<SpriteFont>("Arial");
            font_Arial_Bold = Content.Load<SpriteFont>("Arial_Bold");

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
                        //worst way to get the ID, matches givin string to a list of strings every time this is called
                        //ushort tileType = TileHandler.TileIdByName("Stone");

                        //better way, basically what the option used does behind the scenes
                        //ushort tileType = TileHandler.TileID[typeof(Brick)];

                        //way used, just pass the class you want the ID of, uses a dict to find the ID
                        mainWorld.PlaceTile(GetTileID<Brick>(), tileCoord);
                    }
                    else if (mouseState.RightButton == ButtonState.Pressed)
                    {
                        mainWorld.PlaceTile(GetTileID<Air>(), tileCoord);
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

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);//clearing screen
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);//beginning the main spritebatch


            mainWorld.Draw(spriteBatch);//draws everything in the world

            if (debug)//fps
            {
                spriteBatch.DrawString(font_Arial, "FPS: " + Math.Round(1f / gameTime.ElapsedGameTime.TotalSeconds, 1).ToString(), ScreenSize / 40, Color.LightGoldenrodYellow, default, default, 1f, default, default); ;//position + new Vector2(20, 0) //new Vector2(GameMain.screenWidth / 2, GameMain.screenHeight / 2)
            }

            #region mouse
            spriteBatch.Draw(mouseTexture, mousePos.ToVector2(), null, Color.White, default, default, 2f, default, default);

            foreach(Npc npc in mainWorld.npcs)
            {
                if (npc.Rect.Contains(mousePos.ScreenToWorldCoords()))
                {
                    Vector2 npcNameOffset = font_Arial.MeasureString(npc.displayName);
                    spriteBatch.DrawString(font_Arial, npc.displayName, mousePos.ToVector2(), Color.White, default, new Vector2(npcNameOffset.X / 2.5f, -npcNameOffset.Y), 1f, default, default);//position + new Vector2(20, 0) //new Vector2(GameMain.screenWidth / 2, GameMain.screenHeight / 2)
                }
            }

            Point tileCoord = mousePos.ScreenToTileCoords();

            if(TileHandler.IsPointWithinArray(tileCoord, ref mainWorld.tile))
            {
                string tileName = TileHandler.TileInternalNames[mainWorld.tile[tileCoord.X, tileCoord.Y].type];
                Vector2 tileNameOffset = font_Arial.MeasureString(tileName);

                spriteBatch.DrawString(font_Arial, tileName, mousePos.ToVector2(), Color.White, default, new Vector2(tileNameOffset.X / 2.5f, tileNameOffset.Y), 1f, default, default);
            }
            #endregion

            spriteBatch.End();//ending main spritebatch
        }
    }
}
