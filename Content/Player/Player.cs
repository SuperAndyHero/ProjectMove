using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Design;
using System;
using ProjectMove.Content.Npcs;
using ProjectMove.Content.Tiles;

namespace ProjectMove.Content.Player
{
    public class Player : Entity
    {
        public World currentWorld;

        public const int DefaultWidth = 16;
        public const int DefaultHeight = 24;

        public const float DefaultMaxSpeed = 3.5f;
        public const float DefaultAcceleration = 0.1f;
        public const float DefaultDeceleration = 0.90f;
        public const int   DefaultMaxHealth = 350;
        public const int   DefaultDamageCooldown = 60;
        public const float WallDrag = 0.9f;

        //all this stuff gets set when the player is created
        public float maxSpeed = DefaultMaxSpeed;
        public float acceleration = DefaultAcceleration;
        public float deceleration = DefaultDeceleration;
        public int   health = DefaultMaxHealth;

        public void Initialize()
        {
            size = new Vector2(DefaultWidth, DefaultHeight);
            velocity = Vector2.Zero;
            position = Vector2.Zero;
            oldPosition = Vector2.Zero;
            active = true;
        }

        private Vector2 moveDir = Vector2.Zero;

        public void Update()
        {
            //float frameSkip = (float)gameTime.ElapsedGameTime.TotalSeconds;//frame skip
            KeyboardState keystate = Keyboard.GetState();
            moveDir = Vector2.Zero;
            
            if (keystate.IsKeyDown(Keys.W))
                moveDir += new Vector2(0, -1);
            if (keystate.IsKeyDown(Keys.S))
                moveDir += new Vector2(0, 1);
            if (keystate.IsKeyDown(Keys.A))
                moveDir += new Vector2(-1, 0);
            if (keystate.IsKeyDown(Keys.D))
                moveDir += new Vector2(1, 0);

            //update speed/accel modifiers here or earlier
            if (moveDir != Vector2.Zero)
            {
                float moveDist = (maxSpeed * acceleration);// * frameSkip;//not sure if frameskip is needed, all framerate accounting for will need to be changed
                velocity += (Vector2.Normalize(moveDir) * moveDist);

                if (velocity.Length() > maxSpeed)
                {
                    velocity = Vector2.Normalize(velocity) * maxSpeed;
                }

                Direction = GetDirection();//updated here since it may be used other places in the game
            }
            else if (velocity != Vector2.Zero)
            {
                velocity = Vector2.Normalize(velocity) * (velocity.Length() * deceleration);
            }

            //float steps = (float)Math.Ceiling(velocity.Length() / 2);
            //for (int e = 0; e < steps; e++)
            //{
                //position += (velocity / steps);
                position += velocity;
                TileCollisions();
            //}


            EnemyCollisions();

            if (Math.Abs(velocity.X) < 0.1f)//if velocity is below an amount set it to zero
            {
                velocity.X = 0;
            }

            if (Math.Abs(velocity.Y) < 0.1f)//if velocity is below an amount set it to zero
            {
                velocity.Y = 0;
            }

            UpdateFrame();

            oldPosition = position;
        }

        private void TileCollisions()
        {
            Point centerTile = Center.WorldToTileCoords();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    Point currentTilePos = centerTile + new Point(i, j);
                    if(TileHandler.IsPointWithinArray(currentTilePos, ref currentWorld.wallLayer))
                    {
                        WallTile tile = currentWorld.wallLayer[currentTilePos.X, currentTilePos.Y];
                        if (tile.Base.IsSolid())
                        {
                            foreach (Rectangle tileRect in tile.Base.CollisionRect())
                            {
                                Rectangle testRect = new Rectangle(tileRect.Location + currentTilePos.TileToWorldCoords(), tileRect.Size);

                                //eek
                                if (testRect.Intersects( new Rectangle(new Point((int)position.X, (int)oldPosition.Y), size.ToPoint()) ))
                                {
                                    position.X = oldPosition.X;
                                    velocity.X = 0;
                                    velocity.Y *= WallDrag;
                                }
                                if (testRect.Intersects(new Rectangle(new Point((int)oldPosition.X, (int)position.Y), size.ToPoint())))
                                {
                                    position.Y = oldPosition.Y;
                                    velocity.Y = 0;
                                    velocity.X *= WallDrag;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void EnemyCollisions()
        {
            foreach(Npc npc in GameMain.mainWorld.npcs)
            {
                if(Rect.Intersects(npc.Rect))
                {
                    health--;
                }
            }
        }

        public byte Direction = 0;
        private byte GetDirection()
        {
            byte dirX = (byte)(moveDir.X < 0 ? 3 : 2);
            byte dirY = (byte)(moveDir.Y < 0 ? 1 : 0);
            return Math.Abs(moveDir.Y) >= Math.Abs(moveDir.X) ? dirY : dirX;
        }

        public Vector2 Frame = Vector2.Zero;//not const so its initalized when class is initalized, so it can be set here
        public void UpdateFrame()//this could be called from Draw() or Update()
        {
            Frame.X = Direction * FrameWidth;//direction is updated in Update() with the movement code
            if (moveDir != Vector2.Zero)
                Frame.Y = ((GameMain.mainUpdateCount / 8) % 4) * FrameHeight;
            else
                Frame.Y = 0;
        }


        public const int FrameCountX = 4;
        public const int FrameCountY = 4;
        public int FrameWidth { get { return GameMain.playerTexture.Width / FrameCountX; } }
        public int FrameHeight { get { return GameMain.playerTexture.Height / FrameCountY; } }
        public Vector2 FrameSize { get { return new Vector2(FrameWidth, FrameHeight); } }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle playerFrame = new Rectangle(Frame.ToPoint(), FrameSize.ToPoint());
            //sprite
            spriteBatch.Draw(GameMain.playerTexture, Center.WorldToScreenCoords(), playerFrame, Color.White, default, FrameSize.Half(), 2f, default, default);

            //health text
            string healthStr = health.ToString();
            Vector2 textSize = GameMain.font_Arial_Bold.MeasureString(healthStr);
            spriteBatch.DrawString(GameMain.font_Arial_Bold, healthStr, Rect.Center.WorldToScreenCoords(), Color.White, default, new Vector2(textSize.X / 2, textSize.Y + size.Y / 2), 1, default, default);

            //debug
            if (GameMain.debug)
            {
                //velocity text
                spriteBatch.DrawString(GameMain.font_Arial, velocity.ToString(), position.WorldToScreenCoords(), Color.LightGoldenrodYellow, default, FrameSize.Half() - new Vector2(20, 0), 1f, default, default);//position + new Vector2(20, 0) //new Vector2(GameMain.screenWidth / 2, GameMain.screenHeight / 2)
                
                //hitbox
                spriteBatch.Draw(GameMain.debugTexture, Rect.WorldToScreenCoords(), Color.Red);

                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        Point tilePos = Center.WorldToTileCoords() + new Point(i, j);
                        if (TileHandler.IsPointWithinArray(tilePos, ref currentWorld.wallLayer))
                        {
                            WallTile tile = currentWorld.wallLayer[tilePos.X, tilePos.Y];
                            if (tile.Base.IsSolid())
                            {
                                spriteBatch.Draw(GameMain.debugTexture, new Rectangle((Center.WorldToTileCoords() + new Point(i, j)).TileToScreenCoords(), new Point(TileHandler.tileSize)), new Color(0, j * 16, i * 16));
                            }
                        }  
                    }
                }
            }
        }
    }
}